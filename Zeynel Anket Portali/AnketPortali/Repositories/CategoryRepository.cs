using AnketPortali.Models;

namespace AnketPortali.Repositories
{
    public class CategoryRepository : GenericRepository<Category>
    {
        public CategoryRepository(AppDbContext context) : base(context, context.Categories)
        {
        }
    }
} 