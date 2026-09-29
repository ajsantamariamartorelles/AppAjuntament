using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppAjuntament.Models;
using System.Collections.Generic;
using System.Linq;

namespace AppAjuntament.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuxiliarsController : ControllerBase
    {
        // Simulació de dades auxiliars (substitueix per accés real a MySQL)
        private static List<AuxiliarModel> _auxiliars = new List<AuxiliarModel>
        {
            new AuxiliarModel { Id = 1, Nom = "Categoria A", Tipus = "Tipus 1" }
        };

        // Viewer pot llegir
        [HttpGet]
        [Authorize(Policy = "ViewerOnly")]
        public IActionResult GetAuxiliars()
        {
            return Ok(_auxiliars);
        }

        // Només Admin pot crear/modificar auxiliars
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult CreateAuxiliar([FromBody] AuxiliarModel model)
        {
            model.Id = _auxiliars.Max(a => a.Id) + 1;
            _auxiliars.Add(model);
            return CreatedAtAction(nameof(GetAuxiliars), new { id = model.Id }, model);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult UpdateAuxiliar(int id, [FromBody] AuxiliarModel model)
        {
            var auxiliar = _auxiliars.FirstOrDefault(a => a.Id == id);
            if (auxiliar == null) return NotFound();
            auxiliar.Nom = model.Nom;
            auxiliar.Tipus = model.Tipus;
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult DeleteAuxiliar(int id)
        {
            var auxiliar = _auxiliars.FirstOrDefault(a => a.Id == id);
            if (auxiliar == null) return NotFound();
            _auxiliars.Remove(auxiliar);
            return NoContent();
        }
    }

    // Model simple per auxiliars (afegeix a Models/Models.cs si vols)
    public class AuxiliarModel
    {
        public int Id { get; set; }
        public required string Nom { get; set; }
        public required string Tipus { get; set; }
    }
}