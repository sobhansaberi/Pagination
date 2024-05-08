using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Application.Services;

public class TransactionReportService : ITransactionReportService
{
    private readonly AppDbContext _dbContext;
    private readonly IDbConnection _dbConnection;

    public TransactionReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbConnection = dbContext.Database.GetDbConnection();
    }

    public async Task<PaginatedList<TransactionReportResponseDto>> TransactionsReport(TransactionReportRequestDto request, TransactionReportPaginated paginated)
    {
        var queryable = _dbContext.FinancialTransactions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(request.SourcePan)
            && !string.IsNullOrWhiteSpace(request.SourcePan))
            queryable = queryable.Where(t => t.SourcePan == request.SourcePan);

        if (!string.IsNullOrEmpty(request.DestinationPan)
            && !string.IsNullOrWhiteSpace(request.DestinationPan))
            queryable = queryable.Where(t => t.DestinationPan == request.DestinationPan);

        if (request.Amount != 0)
            queryable = queryable.Where(t => t.Amount == request.Amount);

        var commonConditions = queryable.Where(m => m.TerminalNumber == request.TerminalNumber
                                        && m.RegistrationDate.Date.Date == request.Date.Date
                                        && m.RegistrationDate.Hour == request.Date.Hour);

        if (!request.TransactionStatus.Equals(TransactionStatus.None))
        {
            var transactionStatus = (byte)(request.TransactionStatus);

            queryable = commonConditions.Where(m => m.TransactionStatus == transactionStatus);
        }
        else
        {
            queryable = commonConditions;
        }

        PaginatedList<TransactionReportResponseDto> result = await queryable
           .OrderBy(e => e.RegistrationDate)
           .Select(e => new TransactionReportResponseDto(e.SourcePan,
           e.DestinationPan,
           e.Amount,
           e.ReferenceNumber,
           e.SourceAddress,
           e.TrackingNumber,
           e.TransactionStatus,
           e.TransactionType,
           e.RegistrationDate))
           .PaginatedListAsync(paginated.PageNumber, paginated.PageSize);

        return result;
    }
}
