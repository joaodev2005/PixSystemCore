using MediatR;
using PixSystemCore.Application.Commands;
using PixSystemCore.Domain.Entities;
using PixSystemCore.Domain.Exceptions;
using PixSystemCore.Domain.Interfaces;

namespace PixSystemCore.Application.Handlers;

public class OpenAccountHandler : IRequestHandler<OpenAccountCommand, OpenAccountResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OpenAccountHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OpenAccountResponse> Handle(OpenAccountCommand request, CancellationToken cancellationToken)
    {
        var existingAccount = await _accountRepository.GetByPixKeyAsync(request.PixKey);
        if (existingAccount != null)
            throw new DomainException("Já existe uma conta com esta chave Pix");

        var account = new Account(request.HolderName, request.PixKey, request.KeyType);

        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new OpenAccountResponse
        {
            AccountId = account.Id,
            HolderName = account.HolderName,
            PixKey = account.PixKey,
            InitialBalance = account.Balance
        };
    }
}
