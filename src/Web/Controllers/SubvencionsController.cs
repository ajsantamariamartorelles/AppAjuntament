using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppAjuntament.Models;
using System.Collections.Generic;
using System.Linq;

namespace AppAjuntament.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubvencionsController : ControllerBase
    {
        // Simulació de dades (substitueix per accés real a MySQL via DbContext)
        private static List<SubvencioModel> _subvencions = new List<SubvencioModel>
        {
            new SubvencioModel { Id = 1, Nom = "Subvenció A", Descripcio = "Descripció A" }
        };

        // Viewer pot llegir
        [HttpGet]
        [Authorize(Policy = "ViewerOnly")]
        public IActionResult GetSubvencions()
        {
            return Ok(_subvencions);
        }

        // Editor/Admin pot crear
        [HttpPost]
        [Authorize(Policy = "EditorOrAdmin")]
        public IActionResult CreateSubvencio([FromBody] SubvencioModel model)
        {
            model.Id = _subvencions.Max(s => s.Id) + 1;
            _subvencions.Add(model);
            return CreatedAtAction(nameof(GetSubvencions), new { id = model.Id }, model);
        }

        // Editor/Admin pot modificar
        [HttpPut("{id}")]
        [Authorize(Policy = "EditorOrAdmin")]
        public IActionResult UpdateSubvencio(int id, [FromBody] SubvencioModel model)
        {
            var subvencio = _subvencions.FirstOrDefault(s => s.Id == id);
            if (subvencio == null) return NotFound();
            subvencio.Nom = model.Nom;
            subvencio.Descripcio = model.Descripcio;
            return NoContent();
        }

        // Només Admin pot eliminar
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult DeleteSubvencio(int id)
        {
            var subvencio = _subvencions.FirstOrDefault(s => s.Id == id);
            if (subvencio == null) return NotFound();
            _subvencions.Remove(subvencio);
            return NoContent();
        }
    }

    // Model simple per subvencions (afegeix a Models/Models.cs si vols)
    public class SubvencioModel
    {
        public int Id { get; set; }
        public required string Nom { get; set; }
        public required string Descripcio { get; set; }
    }
}