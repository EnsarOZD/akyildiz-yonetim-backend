using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Application.DTOs;
using AkyildizYonetim.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AkyildizYonetim.Application.Payments.Commands.CreatePayment;

public record CreatePaymentCommand : IRequest<Result<PaymentDto>>
{
	public decimal Amount { get; init; }
	public PaymentType Type { get; init; }
	public PaymentStatus Status { get; init; }
	public PaymentMethod Method { get; init; }
	public string? BankName { get; init; }
	public DateTime PaymentDate { get; init; }
	public string? Description { get; init; }
	public string? ReceiptNumber { get; init; }
	public Guid? OwnerId { get; init; }
	public Guid? TenantId { get; init; }
}

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;
	private readonly IMediator _mediator;
	private readonly ICurrentUserService _currentUserService;

	public CreatePaymentCommandHandler(IApplicationDbContext context, IMapper mapper, IMediator mediator, ICurrentUserService currentUserService)
	{
		_context = context;
		_mapper = mapper;
		_mediator = mediator;
		_currentUserService = currentUserService;
	}

	public async Task<Result<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
	{
		var payment = new Payment
		{
			Id = Guid.NewGuid(),
			Amount = request.Amount,
			Type = request.Type,
			Status = request.Status,
			Method = request.Method,
			BankName = request.BankName,
			PaymentDate = request.PaymentDate,
			Description = request.Description,
			ReceiptNumber = request.ReceiptNumber,
			OwnerId = request.OwnerId,
			TenantId = request.TenantId,
			CreatedAt = DateTime.UtcNow
		};

		_context.Payments.Add(payment);
		await _context.SaveChangesAsync(cancellationToken);

		// 1. Notify Managers about new payment
		await _mediator.Publish(new AkyildizYonetim.Domain.Events.PaymentCreatedEvent(
			payment.Id,
			payment.TenantId,
			payment.Amount), cancellationToken);

		// 2. Notify Tenant if payment is confirmed
		if (payment.TenantId.HasValue && request.Status == AkyildizYonetim.Domain.Entities.PaymentStatus.Completed)
		{
			await _mediator.Publish(new AkyildizYonetim.Domain.Events.PaymentConfirmedEvent(
				payment.Id,
				payment.TenantId.Value,
				payment.Amount), cancellationToken);
		}

		var paymentDto = _mapper.Map<PaymentDto>(payment);
		return Result<PaymentDto>.Success(paymentDto);
	}
}