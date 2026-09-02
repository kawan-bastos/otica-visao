using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OticaVisao.Domain.Catalog;
using OticaVisao.Domain.Customers;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Tests.Persistence;

public sealed class ApplicationDbContextTests
{
    [Fact]
    public void ModelMapsFrameCatalogToPostgresqlSchema()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=otica_visao_tests;Username=postgres")
            .Options;

        using var context = new ApplicationDbContext(options);
        var entity = context.Model.FindEntityType(typeof(Frame));

        Assert.NotNull(entity);
        Assert.Equal("frames", entity.GetTableName());

        var table = StoreObjectIdentifier.Table("frames", null);
        Assert.Equal("code", entity.FindProperty(nameof(Frame.Code))?.GetColumnName(table));
        Assert.Equal("numeric(10,2)", entity.FindProperty(nameof(Frame.Price))?.GetColumnType());
        Assert.Equal("stock_quantity", entity.FindProperty(nameof(Frame.StockQuantity))?.GetColumnName(table));
        Assert.Equal("image_file_name", entity.FindProperty(nameof(Frame.ImageFileName))?.GetColumnName(table));

        var codeIndex = entity.GetIndexes().Single(index =>
            index.Properties.Single().Name == nameof(Frame.Code));

        Assert.True(codeIndex.IsUnique);
        Assert.Null(entity.FindProperty(nameof(Frame.IsAvailable)));
    }

    [Fact]
    public void ModelMapsCustomersToPostgresqlSchema()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=otica_visao_tests;Username=postgres").Options;
        using var context = new ApplicationDbContext(options);
        var entity = context.Model.FindEntityType(typeof(Customer));

        Assert.NotNull(entity);
        Assert.Equal("customers", entity.GetTableName());
        var table = StoreObjectIdentifier.Table("customers", null);
        Assert.Equal("name", entity.FindProperty(nameof(Customer.Name))?.GetColumnName(table));
        Assert.Equal(120, entity.FindProperty(nameof(Customer.Name))?.GetMaxLength());
        Assert.Equal("phone", entity.FindProperty(nameof(Customer.Phone))?.GetColumnName(table));
        Assert.Equal("cpf", entity.FindProperty(nameof(Customer.Cpf))?.GetColumnName(table));
        Assert.True(entity.GetIndexes().Single(index => index.Properties.Single().Name == nameof(Customer.Cpf)).IsUnique);
    }
}
