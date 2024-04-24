using boutique.Models;
using boutique.Services;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace boutique.Controllers
{
    public class ProduitsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public ProduitsController(ApplicationDbContext context, IWebHostEnvironment environment) {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            // Ajouter une vue avec le Razor View-Empty
            // Ajouter les produits dans une liste pour la vue
            var produits = context.Produits.OrderByDescending(p => p.Id).ToList();
            return View(produits);
        }

        // Create() est l'action qui est dans l'index en aspn-action
        [HttpGet]
        [Route("Produits/Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Produits/Create")]
        public IActionResult Create(ProduitDto produitDto)
        {
            return Create(produitDto, context);
        }

        // Pour permettre d'envoyer les données saisies par l'utilisateur au serveur
        [HttpPost]
        [ValidateAntiForgeryToken] // Cette attribut Protége contre les attaques CSRF
        public IActionResult Create(ProduitDto produitDto, ApplicationDbContext context)
        {
            // Verifier si l'utilisateur a mis une image
            if (produitDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "L'image est requis");
            }

            // Voir si on a une erreur dans le model
            if(!ModelState.IsValid)
            {
                return View(produitDto);
            }
            // L'enregistrement de l'image
            string nouveauImage = DateTime.Now.ToString("yyyyMMssHHmmssfff");
            nouveauImage += Path.GetExtension(produitDto.ImageFile!.FileName);

            // Donne le chemin complet de l'enregistrement de l'image
            string cheminImage = environment.WebRootPath + "/produits/" + nouveauImage;
            using(var stream = System.IO.File.Create(cheminImage))
            {
                produitDto.ImageFile.CopyTo(stream);
            }

            // Ajout le produit dans la BDD
            Produit produit = new Produit()
            {
                Name = produitDto.Name,
                Brand = produitDto.Brand,
                Category = produitDto.Category,
                Price = produitDto.Price,
                Description = produitDto.Description,
                ImageFileName = nouveauImage,
                CreatedAt = DateTime.Now,
            };

            // Ajout dans la BDD
            context.Produits.Add(produit);
            context.SaveChanges();

            // Si tout est Ok, on redirige le User à la page Index pour  lui afficher le produit nouvellement créer
            return RedirectToAction("Index", "Produits");
        }

        //------------------------Pour éditer un produit, créant l'action Edit qui est
        // Localisé dans le Create.cshtml
        // -----------On indique l'id de produit à modifier 

        public IActionResult Edit(int id)
        {
            var produit = context.Produits.Find(id);

            // Verification pour savoir si un ID existe pour un produit lors du click
            // "Produits" est le produit du Controller

            if (produit == null)
            {
                return RedirectToAction("index", "Produits");
            }

            // Par contre si on trouve un ID pour le produit à modifier
            // je vais créer un objet de type ProduitDto qui utilise le context du produit 
            // mentionné si dessus "var produit = context.Produits.Find(id);"

            // Création du ProduitDto depuis la variable produit que nous avons obtenu depuis la BDD
            var produitDto = new ProduitDto()
            {
                Name = produit.Name,
                Brand = produit.Brand,
                Category = produit.Category,
                Price = produit.Price,
                Description = produit.Description,
            };

            ViewData["ProduitId"] = produit.Id;
            ViewData["ImageFileName"] = produit.ImageFileName;
            ViewData["CreatedAt"] = produit.CreatedAt.ToString("MM/dd/yyyy");

            context.Produits.FirstOrDefault(p=>produit.Id == p.Id).Name = "Test12";
            

            return View(produitDto);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProduitDto produitDto)
        {
            var produit = context.Produits.Find(id);

            if (produit == null)
            {
                return RedirectToAction("index", "Produits");
            }
            // Si le submit de update fonctionne

            if (!ModelState.IsValid)
            {
                ViewData["ProduitId"] = produit.Id;
                ViewData["ImageFileName"] = produit.ImageFileName;
                ViewData["CreatedAt"] = produit.CreatedAt.ToString("MM/dd/yyyy");
                return View(produitDto);
            }

            string nouveauImage = DateTime.Now.ToString("yyyyMMssHHmmssfff");
            nouveauImage += Path.GetExtension(produitDto.ImageFile!.FileName);

            string cheminImage = environment.WebRootPath + "/produits/" + nouveauImage;
            using (var stream = System.IO.File.Create(cheminImage))
            {
                produitDto.ImageFile.CopyTo(stream);
            }

            // Supprimer l'ancienne image
            string ancienneImage = environment.WebRootPath + "/produits/" + produit.ImageFileName;
            System.IO.File.Delete(ancienneImage);
        
            //Mise à jour de l'image
            produit.Name = produitDto.Name;
            produit.Brand = produitDto.Brand;
            produit.Category = produitDto.Category;
            produit.Price = produitDto.Price;
            produit.Description = produitDto.Description;
            produit.ImageFileName = nouveauImage;

            context.SaveChanges();

            return RedirectToAction("Index", "Produits");
        }

        // L'action de suppression d'un produit 

        public IActionResult Delete(int id)
        {
            // Chercher si l'ID du produit existe dans la BDD
            var product = context.Produits.Find(id);
            // S'il n'existe pas, on va diriger l'utilisateur vers la page Index
            if (product == null)
            {
                return RedirectToAction("Index", "Produits");
            }

            // Supprimer l'image
            string cheminImage = environment.WebRootPath + "/produits/" + product.ImageFileName;
            System.IO.File.Delete($"{cheminImage}");

            // Supprimer le produit depuis la BDD
            context.Produits.Remove(product);
            context.SaveChanges(true);

            return RedirectToAction("Index", "Produits");
        }
    }
}
