using GraphQL.Models;
using GraphQL.Data;

namespace GraphQL.GraphQL.DataItem;
public class ItemType : ObjectType<ItemData>
{
    protected override void Configure(IObjectTypeDescriptor<ItemData> descriptor)
    {
        descriptor.Description("Used to define todo item for a specific list");

        descriptor.Field(x => x.ItemList)
                    .ResolveWith<Resolvers>(p => p.GetList(default!, default!))
                    .UseDbContext<ApiDbContext>()
                    .Description("This is the list that the item belongs to");
    }

    private class Resolvers
    {
        public ItemList GetList(ItemData item, [ScopedService] ApiDbContext context)
        {
            return context.Lists.FirstOrDefault(x => x.Id == item.ListId);
        }
    }
}