using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppAjuntament.Models.Cursets.Dto;
using AppAjuntament.Services.Cursets;

namespace AppAjuntament.Controllers.Cursets
{
    /// <summary>
    /// Administració del mòdul Cursets des de la web d'AppAjuntament (personal de
    /// l'ajuntament autenticat amb Azure AD, NO amb el Bearer de Cursets).
    /// Aquí es donen d'alta les credencials de les professores i es consulta la
    /// liquidació (qui deu què) dels cursets amb cost.
    /// </summary>
    [ApiController]
    [Route("api/cursets/admin")]
    [Authorize(Policy = "EditorOrAdmin")]
    public class CursetsAdminController : ControllerBase
    {
        private readonly ICursetsAuthService _authService;
        private readonly ICursetsLiquidacioService _liquidacioService;
        private readonly ICursetsCatalogService _catalogService;
        private readonly ICursetsSessionsService _sessionsService;

        public CursetsAdminController(
            ICursetsAuthService authService,
            ICursetsLiquidacioService liquidacioService,
            ICursetsCatalogService catalogService,
            ICursetsSessionsService sessionsService)
        {
            _authService = authService;
            _liquidacioService = liquidacioService;
            _catalogService = catalogService;
            _sessionsService = sessionsService;
        }

        /// <summary>Llistat de professores (per defecte inclou les que estan de baixa).</summary>
        [HttpGet("professores")]
        public async Task<IActionResult> LlistarProfessores([FromQuery] bool incloureInactives = true)
        {
            var professores = await _authService.LlistarProfessoresAsync(incloureInactives);
            return Ok(professores);
        }

        /// <summary>Crea o actualitza la contrasenya d'una professora que després farà servir l'app MAUI.</summary>
        [HttpPost("professores")]
        public async Task<IActionResult> CrearOActualitzarProfessora([FromBody] CrearProfessoraRequest request)
        {
            try
            {
                var usuari = await _authService.CrearOActualitzarProfessoraAsync(request.Nom, request.Email, request.Password);
                return Ok(new { usuari.Id, usuari.Nom, usuari.Email });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Actualitza el nom i l'email d'una professora existent.</summary>
        [HttpPut("professores/{id:int}")]
        public async Task<IActionResult> ActualitzarProfessora(int id, [FromBody] ActualitzarProfessoraRequest request)
        {
            try
            {
                var usuari = await _authService.ActualitzarProfessoraAsync(id, request.Nom, request.Email);
                return Ok(new { usuari.Id, usuari.Nom, usuari.Email });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Dona d'alta o de baixa una professora (sense esborrar l'usuari).</summary>
        [HttpPost("professores/{id:int}/estat")]
        public async Task<IActionResult> CanviarEstatProfessora(int id, [FromBody] CanviarEstatProfessoraRequest request)
        {
            try
            {
                await _authService.CanviarEstatProfessoraAsync(id, request.Activa);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Assigna una contrasenya nova a una professora.</summary>
        [HttpPost("professores/{id:int}/password")]
        public async Task<IActionResult> ReiniciarPassword(int id, [FromBody] ReiniciarPasswordProfessoraRequest request)
        {
            try
            {
                await _authService.ReiniciarPasswordAsync(id, request.Password);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== Regidors / controladors ====================

        /// <summary>Regidors del mandat actual (per assignar-los com a responsables i donar-los accés a l'app).</summary>
        [HttpGet("regidors/vigents")]
        public async Task<IActionResult> RegidorsVigents()
            => Ok(await _authService.GetRegidorsVigentsAsync());

        /// <summary>Estat d'accés a l'app d'un regidor.</summary>
        [HttpGet("regidors/{id:int}/acces")]
        public async Task<IActionResult> AccesRegidor(int id)
        {
            var acces = await _authService.GetAccesRegidorAsync(id);
            return acces is null ? NotFound() : Ok(acces);
        }

        /// <summary>Dona (o actualitza) l'accés a l'app d'un regidor: email + contrasenya.</summary>
        [HttpPost("regidors/{id:int}/acces")]
        public async Task<IActionResult> DonarAccesRegidor(int id, [FromBody] DonarAccesControladorRequest request)
        {
            try
            {
                await _authService.DonarOActualitzarAccesAsync(id, request.Email, request.Password);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Treu l'accés a l'app d'un regidor (manté l'email).</summary>
        [HttpDelete("regidors/{id:int}/acces")]
        public async Task<IActionResult> TreureAccesRegidor(int id)
        {
            try
            {
                await _authService.TreureAccesAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Esborra l'email del regidor (i, per força, l'accés a l'app).</summary>
        [HttpDelete("regidors/{id:int}/email")]
        public async Task<IActionResult> EliminarEmailRegidor(int id)
        {
            try
            {
                await _authService.EliminarEmailAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== Temàtiques ====================

        [HttpGet("tipus")]
        public async Task<IActionResult> LlistarTipus([FromQuery] bool nomesActius = false)
            => Ok(await _catalogService.GetTipusCursetsAsync(nomesActius));

        [HttpPost("tipus")]
        public async Task<IActionResult> CrearTipus([FromBody] CrearTipusCursetRequest request)
        {
            try
            {
                var tipus = await _catalogService.CrearTipusCursetAsync(request);
                return Ok(new { tipus.Id, tipus.Nom });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== Cursets ====================

        [HttpGet("cursets")]
        public async Task<IActionResult> LlistarCursets()
            => Ok(await _catalogService.GetCursetsAdminAsync());

        [HttpGet("cursets/{id:int}")]
        public async Task<IActionResult> GetCurset(int id)
        {
            var curset = await _catalogService.GetCursetAsync(id);
            return curset is null ? NotFound() : Ok(curset);
        }

        [HttpPost("cursets")]
        public async Task<IActionResult> CrearCurset([FromBody] CrearCursetRequest request)
        {
            try
            {
                var curset = await _catalogService.CrearCursetAsync(request);
                return Ok(new { curset.Id, curset.Nom });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("cursets/{id:int}")]
        public async Task<IActionResult> ActualitzarCurset(int id, [FromBody] ActualitzarCursetRequest request)
        {
            try
            {
                var curset = await _catalogService.ActualitzarCursetAsync(id, request);
                return Ok(new { curset.Id, curset.Nom });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("cursets/{id:int}/estat")]
        public async Task<IActionResult> CanviarEstatCurset(int id, [FromBody] CanviarEstatCursetRequest request)
        {
            try
            {
                await _catalogService.CanviarEstatCursetAsync(id, request.Actiu);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== Alumnes ====================

        [HttpGet("alumnes")]
        public async Task<IActionResult> LlistarAlumnes([FromQuery] bool incloureInactius = true)
            => Ok(await _catalogService.GetAlumnesAsync(incloureInactius));

        [HttpGet("alumnes/{id:int}")]
        public async Task<IActionResult> GetAlumne(int id)
        {
            var alumne = await _catalogService.GetAlumneAsync(id);
            return alumne is null ? NotFound() : Ok(alumne);
        }

        [HttpGet("alumnes/{id:int}/fitxa")]
        public async Task<IActionResult> GetAlumneFitxa(int id)
        {
            var fitxa = await _catalogService.GetAlumneFitxaAsync(id);
            return fitxa is null ? NotFound() : Ok(fitxa);
        }

        [HttpPost("alumnes/{id:int}/inscripcions")]
        public async Task<IActionResult> AfegirInscripcio(int id, [FromBody] AfegirInscripcioRequest request)
        {
            try
            {
                await _catalogService.AfegirInscripcioAsync(id, request.CursetId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("alumnes/{id:int}/inscripcions/estat")]
        public async Task<IActionResult> CanviarEstatInscripcio(int id, [FromBody] CanviarEstatInscripcioRequest request)
        {
            if (!Enum.TryParse<AppAjuntament.Models.Cursets.EstatInscripcio>(request.NouEstat, out var nou))
                return BadRequest(new { message = "Estat d'inscripció no vàlid." });
            try
            {
                await _catalogService.CanviarEstatInscripcioAsync(id, request.CursetId, nou);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("cursets/{id:int}/sorteig")]
        public async Task<IActionResult> GetSorteigEstat(int id)
        {
            try
            {
                return Ok(await _catalogService.GetSorteigEstatAsync(id));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("cursets/{id:int}/sorteig")]
        public async Task<IActionResult> FerSorteig(int id)
        {
            try
            {
                return Ok(await _catalogService.FerSorteigAsync(id));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("alumnes")]
        public async Task<IActionResult> CrearAlumne([FromBody] CrearAlumneRequest request)
        {
            try
            {
                var alumne = await _catalogService.CrearAlumneAsync(request);
                return Ok(new { alumne.Id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("alumnes/{id:int}")]
        public async Task<IActionResult> ActualitzarAlumne(int id, [FromBody] ActualitzarAlumneRequest request)
        {
            try
            {
                var alumne = await _catalogService.ActualitzarAlumneAsync(id, request);
                return Ok(new { alumne.Id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("alumnes/{id:int}/estat")]
        public async Task<IActionResult> CanviarEstatAlumne(int id, [FromBody] CanviarEstatAlumneRequest request)
        {
            try
            {
                await _catalogService.CanviarEstatAlumneAsync(id, request.Actiu);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== Assistències ====================

        /// <summary>Totes les sessions d'un curset amb qui hi va assistir.</summary>
        [HttpGet("assistencies/curset/{cursetId:int}")]
        public async Task<IActionResult> AssistenciesPerCurset(int cursetId)
            => Ok(await _sessionsService.GetAssistenciesPerCursetAsync(cursetId));

        /// <summary>Historial d'assistència d'un alumne.</summary>
        [HttpGet("assistencies/alumne/{alumneId:int}")]
        public async Task<IActionResult> AssistenciesPerAlumne(int alumneId)
        {
            var resum = await _sessionsService.GetAssistenciesPerAlumneAsync(alumneId);
            return resum is null ? NotFound() : Ok(resum);
        }

        /// <summary>Alumnes que consten apuntats a un curset en una data (per crear-ne la llista).</summary>
        [HttpGet("assistencies/curset/{cursetId:int}/alumnes")]
        public async Task<IActionResult> AlumnesPerLlista(int cursetId, [FromQuery] DateTime data)
            => Ok(await _sessionsService.GetAlumnesPerLlistaAsync(cursetId, data));

        /// <summary>Crea una llista d'assistència des de la web.</summary>
        [HttpPost("assistencies/curset/{cursetId:int}/sessions")]
        public async Task<IActionResult> CrearLlista(int cursetId, [FromBody] CrearLlistaAssistenciaRequest request)
        {
            try
            {
                var id = await _sessionsService.CrearLlistaWebAsync(cursetId, request.Data, request.NotaSessio, request.Alumnes);
                return Ok(new { id });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Elimina una sessió (només si no està liquidada).</summary>
        [HttpDelete("assistencies/sessions/{sessioId:int}")]
        public async Task<IActionResult> EliminarSessio(int sessioId)
        {
            try
            {
                await _sessionsService.EliminarSessioAsync(sessioId);
                return NoContent();
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Inscripcions actives amb absències consecutives (sense cap presència pel mig).</summary>
        [HttpGet("assistencies/absencies")]
        public async Task<IActionResult> AbsenciesConsecutives()
            => Ok(await _sessionsService.GetAbsenciesConsecutivesAsync());

        /// <summary>Dona de baixa les inscripcions seleccionades per absències i avisa per correu.</summary>
        [HttpPost("assistencies/absencies/baixes")]
        public async Task<IActionResult> DonarBaixaPerAbsencies([FromBody] DonarBaixaPerAbsenciesRequest request)
            => Ok(await _sessionsService.DonarBaixaPerAbsenciesAsync(request.Seleccio, request.Missatge));

        /// <summary>Configuració (SMTP + plantilla) del correu de baixa per absències.</summary>
        [HttpGet("assistencies/absencies/config")]
        public async Task<IActionResult> GetEmailAbsenciesConfig()
            => Ok(await _sessionsService.GetEmailConfigAsync());

        [HttpPut("assistencies/absencies/config")]
        public async Task<IActionResult> DesarEmailAbsenciesConfig([FromBody] EmailAbsenciesConfigDto dto)
        {
            await _sessionsService.DesarEmailConfigAsync(dto);
            return NoContent();
        }

        /// <summary>
        /// Llistat de qui deu què en un curset per al període indicat (trimestral,
        /// mensual, bimensual... el període el tries tu). Es factura per classe
        /// impartida, no per assistència personal.
        /// </summary>
        [HttpGet("liquidacio")]
        public async Task<IActionResult> GetLiquidacio([FromQuery] int cursetId, [FromQuery] DateTime dataInici, [FromQuery] DateTime dataFi)
        {
            try
            {
                var liquidacio = await _liquidacioService.GetLiquidacioAsync(cursetId, dataInici, dataFi);
                return Ok(liquidacio);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== Liquidacions registrades (remeses + PDF) ====================

        /// <summary>Previsualitza què es liquidaria per al període indicat, marcant les que ja existeixen.</summary>
        [HttpPost("liquidacions/preview")]
        public async Task<IActionResult> PreviewRemesa([FromBody] PeriodeLiquidacioRequest req)
        {
            try { return Ok(await _liquidacioService.PreviewRemesaAsync(req)); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Emet la remesa de liquidacions per al període (Mode = "NomesNoves" | "ReferTot").</summary>
        [HttpPost("liquidacions/emetre")]
        public async Task<IActionResult> EmetreRemesa([FromBody] EmetreRemesaRequest req)
        {
            try { return Ok(await _liquidacioService.EmetreRemesaAsync(req, null)); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Remeses emeses (més recent primer).</summary>
        [HttpGet("liquidacions/remeses")]
        public async Task<IActionResult> LlistarRemeses()
            => Ok(await _liquidacioService.GetRemesesAsync());

        /// <summary>Detall d'una remesa: liquidacions i resum per alumna.</summary>
        [HttpGet("liquidacions/remeses/{remesaId:int}")]
        public async Task<IActionResult> DetallRemesa(int remesaId)
        {
            var d = await _liquidacioService.GetRemesaAsync(remesaId);
            return d is null ? NotFound() : Ok(d);
        }

        /// <summary>ZIP amb un PDF per alumna de tota la remesa.</summary>
        [HttpGet("liquidacions/remeses/{remesaId:int}/zip")]
        public async Task<IActionResult> ZipRemesa(int remesaId)
        {
            try
            {
                var bytes = await _liquidacioService.GetZipRemesaAsync(remesaId);
                return File(bytes, "application/zip", $"liquidacions-remesa-{remesaId}.zip");
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>PDF consolidat d'una alumna dins la remesa (un full per curset).</summary>
        [HttpGet("liquidacions/remeses/{remesaId:int}/alumnes/{alumneId:int}/pdf")]
        public async Task<IActionResult> PdfAlumna(int remesaId, int alumneId)
        {
            try
            {
                var bytes = await _liquidacioService.GetPdfAlumnaAsync(remesaId, alumneId);
                return File(bytes, "application/pdf", $"liquidacio-{remesaId}-{alumneId}.pdf");
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>PDF d'una liquidació concreta.</summary>
        [HttpGet("liquidacions/{liquidacioId:int}/pdf")]
        public async Task<IActionResult> PdfLiquidacio(int liquidacioId)
        {
            try
            {
                var bytes = await _liquidacioService.GetPdfLiquidacioAsync(liquidacioId);
                return File(bytes, "application/pdf", $"liquidacio-{liquidacioId}.pdf");
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Marca una liquidació com a cobrada (el cobrament real es gestiona fora de l'app).</summary>
        [HttpPost("liquidacions/{liquidacioId:int}/cobrada")]
        public async Task<IActionResult> MarcarCobrada(int liquidacioId, [FromBody] MarcarCobradaRequest req)
        {
            try { await _liquidacioService.MarcarCobradaAsync(liquidacioId, req.Data); return NoContent(); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Anul·la una liquidació (es conserva per a l'auditoria).</summary>
        [HttpPost("liquidacions/{liquidacioId:int}/anullar")]
        public async Task<IActionResult> AnullarLiquidacio(int liquidacioId, [FromBody] AnullarLiquidacioRequest req)
        {
            try { await _liquidacioService.AnullarAsync(liquidacioId, req.Motiu); return NoContent(); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>Textos configurables de la plantilla d'autoliquidació.</summary>
        [HttpGet("liquidacions/config")]
        public async Task<IActionResult> GetLiquidacioConfig()
            => Ok(await _liquidacioService.GetConfigAsync());

        [HttpPut("liquidacions/config")]
        public async Task<IActionResult> DesarLiquidacioConfig([FromBody] LiquidacioConfigDto dto)
        {
            await _liquidacioService.DesarConfigAsync(dto);
            return NoContent();
        }
    }
}
