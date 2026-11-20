using Conciliaciones.Dominio.DTOs.Requests.Conciliation;
using MongoDB.Bson;

namespace Conciliaciones.Infraestructura.Persistence.Documents.Factories
{
    /// <summary>
    /// Arma la plantilla de front (table / accordion) sin reemplazar fuente/datos ya persistidos.
    /// </summary>
    public static class ParametrizacionUiPlantillaBuilder
    {
        public static BsonDocument? Build(parametrizacionUiBodyDto? uiInput)
        {
            if (uiInput == null)
            {
                return null;
            }

            BsonDocument plantilla = new();
            if (uiInput.fuente.Count > 0)
            {
                plantilla["table"] = BuildTable(uiInput.fuente);
            }

            if (uiInput.datos.Count > 0)
            {
                plantilla["accordion"] = BuildAccordion(uiInput.datos);
            }

            return plantilla.ElementCount == 0 ? null : plantilla;
        }

        private static BsonDocument BuildTable(List<fuenteUiColumnaInputDto> columnas)
        {
            string blockId = Guid.NewGuid().ToString();
            BsonArray columns = new() { CheckboxColumn() };
            for (int i = 0; i < columnas.Count; i++)
            {
                columns.Add(DataColumn(columnas[i], i == 0));
            }

            columns.Add(PlantillaEstadoColumna.Build());
            columns.Add(AccionColumn());

            return new BsonDocument
            {
                { "layout", StackLayout(blockId) },
                { "blocks", new BsonArray { TableBlock(blockId, columns, includeSelection: true) } }
            };
        }

        private static BsonDocument BuildAccordion(List<fuenteUiDatosInputDto> datos)
        {
            string accordionId = Guid.NewGuid().ToString();
            string historialId = Guid.NewGuid().ToString();
            BsonArray items = new();
            foreach (fuenteUiDatosInputDto block in datos)
            {
                items.Add(AccordionItem(block));
            }

            return new BsonDocument
            {
                { "revision", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "title", "Detalle de la diferencia" },
                {
                    "layout",
                    new BsonDocument
                    {
                        { "type", "stack" },
                        {
                            "items",
                            new BsonArray
                            {
                                new BsonDocument("blockId", accordionId)
                               // new BsonDocument("blockId", "pestanas")
                            }
                        }
                    }
                },
                {
                    "blocks",
                    new BsonArray
                    {
                        new BsonDocument
                        {
                            { "id", accordionId },
                            { "type", "accordion" },
                            {
                                "dataSource",
                                new BsonDocument(
                                    "path",
                                    "/Conciliaciones/subprocesos/{subProcesoId}/reglas/{regla}/casos/{casoId}")
                            },
                            {
                                "accordion",
                                new BsonDocument
                                {
                                    { "type", "multiple" },
                                    { "items", items },
                                    { "layoutColumns", 2 }
                                }
                            }
                        },
                        //TabsBlock(historialId),
                       // HistorialBlock(historialId)
                    }
                }
            };
        }

        private static BsonDocument AccordionItem(fuenteUiDatosInputDto block)
        {
            BsonArray columns = new();
            foreach (fuenteUiColumnaInputDto col in block.datos)
            {
                columns.Add(AccordionColumn(col));
            }

            return new BsonDocument
            {
                { "id", block.idFuente },
                { "title", block.idFuente },
                {
                    "content",
                    new BsonDocument
                    {
                        { "title", BuildContentTitle(block.datos) },
                        {
                            "emptyState",
                            new BsonDocument("title", "Sin información en este aplicativo.")
                        },
                        { "columns", columns }
                    }
                }
            };
        }

        /// <summary>
        /// En datos solo se persiste el alias: "Número de guía: {numeroGuia}".
        /// </summary>
        private static string BuildContentTitle(List<fuenteUiColumnaInputDto> columnas)
        {
            fuenteUiColumnaInputDto? chosen = columnas.FirstOrDefault(c =>
                c.fuenteDato.EndsWith("numeroGuia", StringComparison.OrdinalIgnoreCase))
                ?? columnas.FirstOrDefault();

            if (chosen == null || string.IsNullOrWhiteSpace(chosen.fuenteDato))
            {
                return string.Empty;
            }

            string alias = FieldId(chosen.fuenteDato);
            string label = string.IsNullOrWhiteSpace(chosen.label) ? alias : chosen.label.Trim();
            return $"{label}: {{{alias}}}";
        }

        private static BsonDocument AccordionColumn(fuenteUiColumnaInputDto col)
        {
            string alias = FieldId(col.fuenteDato);
            BsonDocument column = new()
            {
                { "id", alias },
                { "accessorKey", alias },
                { "header", col.label }
            };

            if (IsNumber(Normalize(col.tipo)))
            {
                column["cell"] = CurrencyCell(highlight: false);
            }

            return column;
        }

        private static BsonDocument DataColumn(fuenteUiColumnaInputDto col, bool primera)
        {
            string tipo = Normalize(col.tipo);
            string alias = FieldId(col.fuenteDato);
            BsonDocument cell = CellFor(tipo, primera);
            BsonDocument column = new()
            {
                { "id", alias },
                { "accessorKey", alias },
                { "header", SortHeader(col.label) },
                { "cell", cell },
                { "meta", new BsonDocument("width", primera ? 216 : 206) },
                { "enableSorting", true },
                { "sortType", SortType(tipo) }
            };

            if (primera)
            {
                column["pin"] = "start";
                column["enableHiding"] = false;
            }

            return column;
        }

        private static BsonDocument CellFor(string tipo, bool primera)
        {
            if (tipo == "date")
            {
                return new BsonDocument
                {
                    { "component", "DateCell" },
                    {
                        "inputs",
                        new BsonDocument
                        {
                            { "dateStyle", "short" },
                            { "timeStyle", "short" },
                            { "timeZone", "America/Bogota" }
                        }
                    }
                };
            }

            if (IsNumber(tipo))
            {
                return CurrencyCell(highlight: false);
            }

            BsonDocument inputs = new();
            if (primera)
            {
                inputs["emphasis"] = "lead";
                inputs["truncate"] = true;
            } else if (tipo is "text" or "string")
            {
                inputs["truncate"] = true;
            }

            BsonDocument cell = new("component", "TextCell");
            if (inputs.ElementCount > 0)
            {
                cell["inputs"] = inputs;
            }

            return cell;
        }

        private static BsonDocument CheckboxColumn() =>
            new()
            {
                { "id", "seleccion" },
                {
                    "header",
                    new BsonDocument
                    {
                        { "component", "SelectAllCheckbox" },
                        { "inputs", new BsonDocument("ariaLabel", "Seleccionar todas las diferencias de la página") }
                    }
                },
                {
                    "cell",
                    new BsonDocument
                    {
                        { "component", "RowCheckbox" },
                        { "inputs", new BsonDocument("ariaLabel", "Seleccionar la diferencia de {conciliacionId}") }
                    }
                },
                { "meta", new BsonDocument("width", 56) },
                { "pin", "start" },
                { "enableHiding", false }
            };

        private static BsonDocument AccionColumn() =>
            new()
            {
                { "id", "accion" },
                { "header", "Acción" },
                {
                    "cell",
                    new BsonDocument
                    {
                        { "component", "LinkCell" },
                        {
                            "inputs",
                            new BsonDocument
                            {
                                { "label", "Ver detalle" },
                                { "ariaLabel", "Ver detalle de la diferencia de {conciliacionId}" },
                                { "route", new BsonArray { "conciliacionId", "{conciliacionId}" } }
                            }
                        }
                    }
                },
                { "meta", new BsonDocument("width", 120) },
                { "pin", "end" },
                { "enableHiding", false }
            };

        private static BsonDocument TableBlock(string blockId, BsonArray columns, bool includeSelection)
        {
            BsonDocument table = new()
            {
                { "rowId", "conciliacionId" },
                {
                    "emptyState",
                    new BsonDocument
                    {
                        { "title", "No hay diferencias por resolver en esta regla." },
                        { "description", "Cuando se detecten diferencias aparecerán aquí." }
                    }
                },
                { "columns", columns }
            };

            if (includeSelection)
            {
                table["selection"] = new BsonDocument
                {
                    { "mode", "multi" },
                    { "persistAcrossPages", true }
                };
            }

            return new BsonDocument
            {
                { "id", blockId },
                { "type", "table" },
                {
                    "dataSource",
                    new BsonDocument
                    {
                        { "path","/Conciliaciones/subprocesos" },
                        { "pagination", new BsonDocument { { "mode", "server" }, { "defaultPageSize", 10 } } },
                        { "sorting", new BsonDocument { { "mode", "client" }, { "multi", false } } }
                    }
                },
                { "table", table }
            };
        }

        private static BsonDocument TabsBlock(string historialId) =>
            new()
            {
                { "id", "pestanas" },
                { "type", "tabs" },
                {
                    "tabs",
                    new BsonDocument
                    {
                        { "defaultTab", "historial" },
                        {
                            "items",
                            new BsonArray
                            {
                                new BsonDocument
                                {
                                    { "id", "historial" },
                                    { "title", "Historial de cambios" },
                                    { "blockId", historialId }
                                }
                            }
                        }
                    }
                }
            };

        private static BsonDocument HistorialBlock(string historialId)
        {
            BsonArray columns = new()
            {
                HistorialText("regla", "Regla", 278),
                HistorialText("correo", "Correo", 224),
                HistorialText("medioPago", "Medio de pago", 204),
                HistorialCurrency("valor", "Valor", 117),
                HistorialDate("fechaCreacion", "Fecha de creación", 211),
                HistorialDate("fechaAprobacion", "Fecha de aprobación", 206)
            };

            BsonDocument block = TableBlock(historialId, columns, includeSelection: false);
            block["title"] = "Historial de cambios";
            block["dataSource"] = new BsonDocument
            {
                { "path", "/Conciliaciones/subprocesos/{subProcesoId}/reglas/{regla}/casos/{casoId}/historial" },
                { "pagination", new BsonDocument("mode", "none") },
                { "sorting", new BsonDocument { { "mode", "client" }, { "multi", false } } }
            };
            block["table"]["rowId"] = "idCambio";
            block["table"]["emptyState"] = new BsonDocument
            {
                { "title", "Este caso aún no tiene cambios." },
                { "description", "Cuando se registre un cambio aparecerá aquí." }
            };
            return block;
        }

        private static BsonDocument HistorialText(string id, string label, int width) =>
            new()
            {
                { "id", id },
                { "accessorKey", id },
                { "header", SortHeader(label) },
                { "cell", new BsonDocument { { "component", "TextCell" }, { "inputs", new BsonDocument("truncate", true) } } },
                { "meta", new BsonDocument("width", width) },
                { "enableSorting", true },
                { "sortType", "text" }
            };

        private static BsonDocument HistorialCurrency(string id, string label, int width) =>
            new()
            {
                { "id", id },
                { "accessorKey", id },
                { "header", SortHeader(label) },
                { "cell", CurrencyCell(false) },
                { "meta", new BsonDocument("width", width) },
                { "enableSorting", true },
                { "sortType", "basic" }
            };

        private static BsonDocument HistorialDate(string id, string label, int width) =>
            new()
            {
                { "id", id },
                { "accessorKey", id },
                { "header", SortHeader(label) },
                {
                    "cell",
                    new BsonDocument
                    {
                        { "component", "DateCell" },
                        { "inputs", new BsonDocument { { "dateStyle", "medium" }, { "timeZone", "America/Bogota" } } }
                    }
                },
                { "meta", new BsonDocument("width", width) },
                { "enableSorting", true },
                { "sortType", "datetime" }
            };

        private static BsonDocument StackLayout(string blockId) =>
            new()
            {
                { "type", "stack" },
                { "items", new BsonArray { new BsonDocument("blockId", blockId) } }
            };

        private static BsonDocument SortHeader(string label) =>
            new()
            {
                { "component", "SortIndicator" },
                { "inputs", new BsonDocument("label", label) }
            };

        private static BsonDocument CurrencyCell(bool highlight)
        {
            BsonDocument inputs = new()
            {
                { "currency", "COP" },
                { "maximumFractionDigits", 0 }
            };
            if (highlight)
            {
                inputs["emphasis"] = "highlight";
            }

            return new BsonDocument
            {
                { "component", "CurrencyCell" },
                { "inputs", inputs }
            };
        }

        /// <summary>Alias de un campo (último tramo tras el punto): es el accessorKey de la plantilla.</summary>
        internal static string FieldId(string fuenteDato)
        {
            string trimmed = fuenteDato.Trim();
            int dot = trimmed.LastIndexOf('.');
            return dot >= 0 && dot < trimmed.Length - 1 ? trimmed[(dot + 1)..] : trimmed;
        }

        private static string SortType(string tipo) => tipo switch
        {
            "date" => "datetime",
            "number" or "currency" => "basic",
            "alphanumeric" => "alphanumeric",
            _ => "text"
        };

        private static bool IsNumber(string tipo) => tipo is "number" or "currency";

        private static string Normalize(string? tipo) =>
            string.IsNullOrWhiteSpace(tipo) ? "text" : tipo.Trim().ToLowerInvariant();
    }
}
