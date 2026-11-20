using System.Text.Json;

namespace Conciliaciones.Aplicacion.Features.Conciliation.Querys.GetParametrizacionPlantilla;

public record GetParametrizacionPlantillaResponse(
    JsonElement Plantilla
);
