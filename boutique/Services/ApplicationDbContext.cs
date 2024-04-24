using boutique.Models;
using Microsoft.EntityFrameworkCore;

namespace boutique.Services
{
    public class ApplicationDbContext : DbContext
    {
        // J'ajoute le constructeur
        public ApplicationDbContext( DbContextOptions options) : base(options)
        {
            
        }

        // J'ajoute les propriétes du produit 
        // Puis aprés dans le console, on crée la migration de la base des données "Add-Migration premiereMigration"
        public DbSet<Produit> Produits { get; set; }
    }
}
