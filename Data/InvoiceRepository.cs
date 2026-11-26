using System.Data;
using Microsoft.Data.SqlClient;

namespace AcuPay_Capstone.Data;

public interface IInvoiceRepository
{
    Task<List<InvoiceDetails>> GetInvoicesAsync(string? status);
    Task<InvoiceDetails?> GetInvoiceByIdAsync(int invoiceId);
    Task<int> InsertInvoiceAsync(string invoiceNumber, string vendorCode, string? poNumber, decimal invoiceAmount, string createdBy);
    Task<bool> UpdateInvoiceMatchAsync(int invoiceId, string status, string matchNote, string? decidedBy);
    Task<bool> DecideInvoiceAsync(int invoiceId, string status, string decidedBy, string? decisionReason);
    Task<AppUser?> GetUserByUsernameAsync(string username);
}

public class InvoiceRepository : IInvoiceRepository
{
    private const int UniqueConstraintError = 2627;
    private const int UniqueIndexError = 2601;
    private const int VendorNotFoundError = 50001;
    private const int PoNotFoundError = 50002;

    private readonly IDbConnectionFactory _connectionFactory;

    public InvoiceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<InvoiceDetails>> GetInvoicesAsync(string? status)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = NewCommand(connection, "dbo.usp_GetInvoices");
        command.Parameters.Add(TextParameter("@Status", status, 20));

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var invoices = new List<InvoiceDetails>();
        while (await reader.ReadAsync())
            invoices.Add(ReadInvoice(reader));

        return invoices;
    }

    public async Task<InvoiceDetails?> GetInvoiceByIdAsync(int invoiceId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = NewCommand(connection, "dbo.usp_GetInvoiceById");
        command.Parameters.Add(IntParameter("@InvoiceId", invoiceId));

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            return ReadInvoice(reader);

        return null;
    }

    public async Task<int> InsertInvoiceAsync(string invoiceNumber, string vendorCode, string? poNumber, decimal invoiceAmount, string createdBy)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await using var command = NewCommand(connection, "dbo.usp_InsertInvoice");
            command.Parameters.Add(TextParameter("@InvoiceNumber", invoiceNumber, 30));
            command.Parameters.Add(TextParameter("@VendorCode", vendorCode, 20));
            command.Parameters.Add(TextParameter("@PoNumber", poNumber, 20));
            command.Parameters.Add(AmountParameter("@InvoiceAmount", invoiceAmount));
            command.Parameters.Add(TextParameter("@CreatedBy", createdBy, 50));

            await connection.OpenAsync();
            object? newId = await command.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }
        catch (SqlException ex) when (ex.Number == UniqueConstraintError || ex.Number == UniqueIndexError)
        {
            throw new DuplicateInvoiceException(invoiceNumber, vendorCode);
        }
        catch (SqlException ex) when (ex.Number == VendorNotFoundError)
        {
            throw new ReferenceNotFoundException("VENDOR_NOT_FOUND", "Vendor code was not found.");
        }
        catch (SqlException ex) when (ex.Number == PoNotFoundError)
        {
            throw new ReferenceNotFoundException("PO_NOT_FOUND", "Purchase order number was not found.");
        }
    }

    public async Task<bool> UpdateInvoiceMatchAsync(int invoiceId, string status, string matchNote, string? decidedBy)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = NewCommand(connection, "dbo.usp_UpdateInvoiceMatch");
        command.Parameters.Add(IntParameter("@InvoiceId", invoiceId));
        command.Parameters.Add(TextParameter("@Status", status, 20));
        command.Parameters.Add(TextParameter("@MatchNote", matchNote, 50));
        command.Parameters.Add(TextParameter("@DecidedBy", decidedBy, 50));

        await connection.OpenAsync();
        object? rowsChanged = await command.ExecuteScalarAsync();
        return Convert.ToInt32(rowsChanged) > 0;
    }

    public async Task<bool> DecideInvoiceAsync(int invoiceId, string status, string decidedBy, string? decisionReason)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = NewCommand(connection, "dbo.usp_DecideInvoice");
        command.Parameters.Add(IntParameter("@InvoiceId", invoiceId));
        command.Parameters.Add(TextParameter("@Status", status, 20));
        command.Parameters.Add(TextParameter("@DecidedBy", decidedBy, 50));
        command.Parameters.Add(TextParameter("@DecisionReason", decisionReason, 500));

        await connection.OpenAsync();
        object? rowsChanged = await command.ExecuteScalarAsync();
        return Convert.ToInt32(rowsChanged) > 0;
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await using var command = NewCommand(connection, "dbo.usp_GetUserByUsername");
        command.Parameters.Add(TextParameter("@Username", username, 50));

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new AppUser
        {
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            Role = reader.GetString(reader.GetOrdinal("Role"))
        };
    }

    private static SqlCommand NewCommand(SqlConnection connection, string procedureName)
    {
        return new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
    }

    private static SqlParameter TextParameter(string name, string? value, int size)
    {
        return new SqlParameter(name, SqlDbType.NVarChar, size) { Value = (object?)value ?? DBNull.Value };
    }

    private static SqlParameter IntParameter(string name, int value)
    {
        return new SqlParameter(name, SqlDbType.Int) { Value = value };
    }

    private static SqlParameter AmountParameter(string name, decimal value)
    {
        return new SqlParameter(name, SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = value };
    }

    private static InvoiceDetails ReadInvoice(SqlDataReader reader)
    {
        return new InvoiceDetails
        {
            InvoiceId = reader.GetInt32(reader.GetOrdinal("InvoiceId")),
            InvoiceNumber = reader.GetString(reader.GetOrdinal("InvoiceNumber")),
            VendorId = reader.GetInt32(reader.GetOrdinal("VendorId")),
            VendorCode = reader.GetString(reader.GetOrdinal("VendorCode")),
            PoId = ReadInt(reader, "PoId"),
            PoNumber = ReadString(reader, "PoNumber"),
            PoVendorId = ReadInt(reader, "PoVendorId"),
            PoAmount = ReadDecimal(reader, "PoAmount"),
            InvoiceAmount = reader.GetDecimal(reader.GetOrdinal("InvoiceAmount")),
            VariancePercent = ReadDecimal(reader, "VariancePercent"),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            MatchNote = ReadString(reader, "MatchNote"),
            CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")),
            DecidedBy = ReadString(reader, "DecidedBy"),
            DecisionReason = ReadString(reader, "DecisionReason")
        };
    }

    private static string? ReadString(SqlDataReader reader, string column)
    {
        int position = reader.GetOrdinal(column);
        return reader.IsDBNull(position) ? null : reader.GetString(position);
    }

    private static int? ReadInt(SqlDataReader reader, string column)
    {
        int position = reader.GetOrdinal(column);
        return reader.IsDBNull(position) ? null : reader.GetInt32(position);
    }

    private static decimal? ReadDecimal(SqlDataReader reader, string column)
    {
        int position = reader.GetOrdinal(column);
        return reader.IsDBNull(position) ? null : reader.GetDecimal(position);
    }
}
