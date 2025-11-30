using FluentAssertions;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Application.ArticleCategoryApp.Query;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using MockQueryable;
using Moq;
using System.Reflection;

namespace ShopTest.ArticleCategoryApp.Query
{
    public class GetArticleCategoriesForAddArticleQueryHandlerTests
    {
        private readonly Mock<IArticleCategoryRepo> _mockRepo;
        private readonly GetArticleCategoriesForAddArticleQueryHandler _handler;

        public GetArticleCategoriesForAddArticleQueryHandlerTests()
        {
            _mockRepo = new Mock<IArticleCategoryRepo>();
            _handler = new GetArticleCategoriesForAddArticleQueryHandler(_mockRepo.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnRootCategories_With_PopulatedSubCategories()
        {
            // Arrange
            var query = new GetArticleCategoriesForAddArticleQuery();

            // 1. ساخت داده‌ها
            // والد 1: دو فرزند دارد
            var root1 = CreateCategory(1, "Root 1", parentId: null);
            var child1_1 = CreateCategory(11, "Child 1-1", parentId: 1);
            var child1_2 = CreateCategory(12, "Child 1-2", parentId: 1);
            // دستی فرزندان را به والد اضافه می‌کنیم (چون در تست حافظه، EF Core نیست که Join بزند)
            AddChildrenToParent(root1, new List<ArticleCategory> { child1_1, child1_2 });

            // والد 2: فرزندی ندارد
            var root2 = CreateCategory(2, "Root 2", parentId: null);

            // والد 3: خودش فرزند است (نباید در لیست اصلی بیاید، اما ما اینجا فقط لیست فلت را ماک می‌کنیم)
            // نکته: در GetAllQueryable همه چیز برمی‌گردد، هندلر باید فیلتر کند.
            var allCategories = new List<ArticleCategory> { root1, child1_1, child1_2, root2 };

            // 2. ماک کردن دیتابیس
            var mockDbSet = allCategories.BuildMock();
            _mockRepo.Setup(x => x.GetAllQueryable()).Returns(mockDbSet);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();

            // 1. فقط باید 2 دسته اصلی (Root1, Root2) برگردند. فرزندان نباید سطح اول باشند.
            result.Value.Should().HaveCount(2);
            result.Value.Select(x => x.Id).Should().Contain(new[] { 1, 2 });
            result.Value.Select(x => x.Id).Should().NotContain(new[] { 11, 12 }); // فرزندان اینجا نیستند

            // 2. بررسی فرزندانِ Root 1
            var firstCat = result.Value.First(c => c.Id == 1);
            firstCat.Title.Should().Be("Root 1");
            firstCat.SubCategories.Should().HaveCount(2); // باید 2 فرزند داشته باشد
            firstCat.SubCategories.Select(s => s.Id).Should().Contain(new[] { 11, 12 });

            // 3. بررسی فرزندانِ Root 2
            var secondCat = result.Value.First(c => c.Id == 2);
            secondCat.SubCategories.Should().BeEmpty(); // باید خالی باشد
        }

        [Fact]
        public async Task Handle_Should_ReturnEmpty_When_NoCategoriesExist()
        {
            // Arrange
            var mockDbSet = new List<ArticleCategory>().BuildMock();
            _mockRepo.Setup(x => x.GetAllQueryable()).Returns(mockDbSet);

            // Act
            var result = await _handler.Handle(new GetArticleCategoriesForAddArticleQuery(), CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().BeEmpty();
        }

        // --- Helper Methods ---

        private ArticleCategory CreateCategory(int id, string title, int? parentId)
        {
            var cat = new ArticleCategory(title, "slug", "img", "alt", parentId);
            ForceSetProperty(cat, "Id", id);

            // مقداردهی اولیه لیست فرزندان (برای جلوگیری از NullReference در تست)
            // اگر در کانستراکتور انتیتی Children = new List<...>() دارید، این خط لازم نیست.
            ForceSetProperty(cat, "Children", new List<ArticleCategory>());

            return cat;
        }

        private void AddChildrenToParent(ArticleCategory parent, List<ArticleCategory> children)
        {
            // چون Children معمولاً ICollection و read-only است، با رفلکشن ست می‌کنیم یا Add می‌کنیم
            // فرض می‌کنیم Children یک کالکشن است که می‌توان به آن Add کرد
            var childrenProp = parent.GetType().GetProperty("Children");
            if (childrenProp != null)
            {
                var collection = childrenProp.GetValue(parent) as ICollection<ArticleCategory>;
                if (collection == null)
                {
                    collection = new List<ArticleCategory>();
                    ForceSetProperty(parent, "Children", collection);
                }

                foreach (var child in children)
                {
                    collection.Add(child);
                }
            }
        }

        private void ForceSetProperty(object obj, string propertyName, object value)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null && prop.CanWrite) { prop.SetValue(obj, value); return; }

                var field = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) { field.SetValue(obj, value); return; }

                type = type.BaseType;
            }
        }
    }
}
