using EldenRingSim.Models;
using EldenRingSim.Services;
using Microsoft.AspNetCore.Mvc;

// Weapon analysis endpoints for damage calc and build opt

namespace EldenRingSim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisEngine _analysisEngine;

        public AnalysisController(IAnalysisEngine analysisEngine)
        {
            _analysisEngine = analysisEngine;
        }

        [HttpPost("weapon/{weaponId}")]
        public async Task<IActionResult> AnalyzeWeapon(string weaponId, [FromBody] PlayerBuild build)
        {
            try
            {
                var result = await _analysisEngine.AnalyzeWeaponAsync(weaponId, build);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("best-weapons")]
        public async Task<IActionResult> FindBestWeapons([FromBody] PlayerBuild build, [FromQuery] int topCount = 10)
        {
            try
            {
                var results = await _analysisEngine.FindBestWeaponsAsync(build, topCount);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("compare")]
        public async Task<IActionResult> CompareWeapons([FromBody] CompareWeaponsRequest request)
        {
            try
            {
                var results = await _analysisEngine.CompareWeaponsAsync(request.WeaponIds, request.Build);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class CompareWeaponsRequest
    {
        public List<string> WeaponIds { get; set; } = new();
        public PlayerBuild Build { get; set; } = new();
    }
}
