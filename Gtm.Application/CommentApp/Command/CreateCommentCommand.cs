using ErrorOr;
using Gtm.Contract.CommentContract.Command;
using Gtm.Domain.CommentDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.CommentApp.Command
{
    public record CreateCommentCommand(CreateCommentDto Command):IRequest<ErrorOr<Success>>;
    public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, ErrorOr<Success>>
    {
        private readonly  ICommentRepo _commentRepo;
        private readonly ICommentValidator _commentValidator;

        public CreateCommentCommandHandler(ICommentRepo commentRepo, ICommentValidator commentValidator)
        {
            _commentRepo = commentRepo;
            _commentValidator = commentValidator;
        }

        public async Task<ErrorOr<Success>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            var validationResults = await _commentValidator.ValidateCreateAsync(request.Command);
            if(validationResults.IsError)
            {
                return validationResults.Errors;
            }
            Comment newComment = new Comment(request.Command.UserId, request.Command.OwnerId, request.Command.CommentFor, request.Command.FullName, request.Command.Email, request.Command.Text, request.Command.ParentId);
             await _commentRepo.AddAsync(newComment);   
           var res= await _commentRepo.SaveChangesAsync(cancellationToken);
            if (res == false)
                return Error.Failure("AddCommnet", "اضافه کردن کامنت به مشکل برخورد");

            return Result.Success;

        }
    }
}
