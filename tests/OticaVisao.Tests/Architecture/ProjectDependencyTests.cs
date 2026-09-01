using System.Reflection;
using OticaVisao.Application.Catalog;
using OticaVisao.Domain;

namespace OticaVisao.Tests.Architecture;

public sealed class ProjectDependencyTests
{
    [Fact]
    public void DomainDoesNotDependOnWebOrInfrastructure()
    {
        var referencedAssemblies = typeof(DomainAssemblyMarker)
            .Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.DoesNotContain("OticaVisao.Web", referencedAssemblies);
        Assert.DoesNotContain("OticaVisao.Infrastructure", referencedAssemblies);
    }

    [Fact]
    public void ApplicationDoesNotDependOnWebOrInfrastructure()
    {
        var referencedAssemblies = typeof(FrameCatalogService)
            .Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.DoesNotContain("OticaVisao.Web", referencedAssemblies);
        Assert.DoesNotContain("OticaVisao.Infrastructure", referencedAssemblies);
    }
}
