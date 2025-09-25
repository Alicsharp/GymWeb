using Gtm.Domain.UserDomain.UserDm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;
using Utility.Domain.Enums;

namespace Gtm.Domain.CommentDomain
{
    public class Comment : BaseEntityCreate<long>
    {
        public Comment(int authorUserId, int targetEntityId, CommentFor commentFor, string? fullName, string? email, string text, long? parentId)
        {

            AuthorUserId = authorUserId;
            TargetEntityId = targetEntityId;
            CommentFor = commentFor;
            FullName = fullName;
            Email = email;
            Text = text.Trim();
            ParentId = parentId;
            Status = CommentStatus.خوانده_نشده;
        }

        public int AuthorUserId { get; private set; }
        public User AuthorUser { get; private set; }
        public int TargetEntityId { get; private set; }
        public CommentFor CommentFor { get; private set; }
        public CommentStatus Status { get; private set; }
        public string? WhyRejected { get; private set; }
        public string? FullName { get; private set; }
        public string? Email { get; private set; }
        public string Text { get; private set; }
        public long? ParentId { get; private set; }
        public DateTime LastUpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; }

        // Navigation Properties
        public Comment? Parent { get; private set; }
        public List<Comment> Children { get; private set; } = new();

        public void Reject(string reason)
        {
            Status = CommentStatus.رد_شده;
            WhyRejected = reason;
            LastUpdatedAt = DateTime.Now;
        }

        public void Approve()
        {
            Status = CommentStatus.تایید_شده;
            WhyRejected = null;
            LastUpdatedAt = DateTime.Now;
        }

        public void Delete() => IsDeleted = true;
    }
}
