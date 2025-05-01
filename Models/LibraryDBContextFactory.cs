using LibraryWPF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class LibraryDBContextFactory : IDesignTimeDbContextFactory<LibraryDBContext>
{
    public LibraryDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LibraryDBContext>();
        optionsBuilder.UseSqlServer("Server=.;Database=Library;Trusted_Connection=True;TrustServerCertificate=True;");

        return new LibraryDBContext(optionsBuilder.Options);
    }
}