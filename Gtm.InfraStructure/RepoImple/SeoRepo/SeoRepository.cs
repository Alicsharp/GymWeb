using Gtm.Application.SeoApp;
using Gtm.Contract.SeoContract.Command;
using Gtm.Domain.SeoDomain;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.SeoRepo
{
    internal class SeoRepository : Repository<Seo,int>, ISeoRepository
    {
        private readonly GtmDbContext _context;

        public SeoRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Seo?> GetSeoAsync(int ownerId, WhereSeo where)
        {
            return await _context.Seos
                .SingleOrDefaultAsync(s => s.OwnerId == ownerId && s.Where == where);
        }

        public async Task<CreateSeo> GetSeoForUpsertAsync(int ownerId, WhereSeo where)
        {
            var seo = await _context.Seos
                .SingleOrDefaultAsync(s => s.OwnerId == ownerId && s.Where == where);

            if (seo == null)
            {
                return new CreateSeo
                {
                    OwnerId = ownerId,
                    Where = where
                };
            }

            return new CreateSeo
            {
                OwnerId = seo.OwnerId,
                Canonical = seo.Canonical,
                IndexPage = seo.IndexPage,
                MetaDescription = seo.MetaDescription,
                MetaKeyWords = seo.MetaKeyWords,
                MetaTitle = seo.MetaTitle,
                Schema = seo.Schema,
                Where = seo.Where
            };
        }

        public async Task<Seo> GetSeoForUiAsync(int ownerId, WhereSeo where, string title)
        {
            var seo = await GetSeoAsync(ownerId, where);

            if (seo == null)
            {
                seo = new Seo(title, "", "", true, "", "", where, ownerId);
                await AddAsync(seo); // CreateAsync از Repository پایه
            }

            return seo;
        }
    }
}
