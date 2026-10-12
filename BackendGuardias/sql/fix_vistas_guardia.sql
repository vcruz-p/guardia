-- =====================================================================
-- FIX VISTAS DEL ESQUEMA `guardia` (v2)
--
-- Objetivo: que las vistas del módulo de guardias NO devuelvan solo el
-- id_persona, sino los DATOS COMPLETOS de la persona (nombre,
-- apellidos, nombre completo, carné de identidad, cargo actual, grado
-- actual y SEXO).
--
-- IMPORTANTE: este script NO crea, modifica ni elimina NADA dentro de
-- las bases `persona`, `estructura` ni `sistemas`. Solo reescribe
-- vistas del esquema `guardia`.
--
-- Esquema real de `persona` (según el DDL proporcionado):
--   persona.persona(id, nombre, apellidos, id_sexo, id_color_piel,
--                   id_persona_inserta, carnet_identidad)
--     -> NO tiene id_cargo / id_grado / id_unidad_organizativa.
--   persona.sexo(id, nombre, descripcion)
--   persona.persona_cargo(id, id_persona, id_cargo_unidad_organizativa,
--                   fecha_introduccion_cargo, fecha_fin_cargo, ...)
--     -> el CARGO y la UNIDAD se obtienen a través de
--        estructura.unidad_organizativa_cargos (cargo + unidad).
--   persona.persona_grado(id, id_persona, id_grado, fecha_insercion,
--                   fecha_fin, ...)
--     -> id_grado -> estructura.grado(id, descripcion).
--
-- Reglas aplicadas:
--   * Cargo vigente  = último registro de persona_cargo sin fecha_fin.
--   * Grado vigente  = último registro de persona_grado sin fecha_fin.
--   * Sexo           = COALESCE(sexo.nombre, sexo.descripcion)
--                      ('Masculino' / 'Femenino').
--   * Las personas sin cargo vigente simplemente NO aparecen en
--     vista_personas_unidades_subordinacion (igual que antes), por lo
--     que no pueden ser planificadas.
-- =====================================================================

USE guardia;

-- ---------------------------------------------------------------------
-- 1. VISTA GRUPO GUARDIA PERSONA
--    Antes: devolvía id_persona "pelado".
--    Ahora: + datos de la persona (nombre, apellidos, nombre_completo,
--           carnet_identidad, id_cargo, cargo_descripcion, id_grado,
--           grado_descripcion, sexo).
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vista_grupo_guardia_persona;

CREATE VIEW vista_grupo_guardia_persona AS
SELECT
    ggp.id_grupo_guardia_persona      AS id_grupo_guardia_persona,
    ggp.id_grupo_guardia              AS id_grupo_guardia,
    gg.nombre                         AS grupo_nombre,
    gg.iniciales                      AS grupo_iniciales,
    ggp.id_persona                    AS id_persona,
    p.nombre                          AS nombre,
    p.apellidos                       AS apellidos,
    CONCAT_WS(' ', p.nombre, p.apellidos) AS persona_nombre_completo,
    p.carnet_identidad                AS carnet_identidad,
    uoc.id_cargo AS id_cargo,
    car.descripcion                   AS cargo_descripcion,
    pg.id_grado                       AS id_grado,
    gr.descripcion                    AS grado_descripcion,
    COALESCE(sx.nombre, sx.descripcion) AS sexo,
    ggp.fecha_inicio                  AS fecha_inicio,
    ggp.fecha_fin                     AS fecha_fin,
    ggp.activo                        AS activo,
    ggp.observacion                   AS observacion,
    ggp.id_persona_crea               AS id_persona_crea,
    ggp.id_persona_actualiza          AS id_persona_actualiza,
    ggp.fecha_hora_creado             AS fecha_hora_creado,
    ggp.fecha_hora_actualizado        AS fecha_hora_actualizado
FROM guardia.grupo_guardia_persona ggp
INNER JOIN guardia.grupo_guardia gg
        ON gg.id_grupo_guardia = ggp.id_grupo_guardia
LEFT JOIN persona.persona p
       ON p.id = ggp.id_persona
-- Cargo vigente (último asignado sin fecha fin).
LEFT JOIN persona.persona_cargo pc
       ON pc.id = (SELECT MAX(pc2.id)
                     FROM persona.persona_cargo pc2
                    WHERE pc2.id_persona = p.id
                      AND pc2.fecha_fin_cargo IS NULL)
LEFT JOIN estructura.unidad_organizativa_cargos uoc
       ON uoc.id = pc.id_cargo_unidad_organizativa
LEFT JOIN estructura.cargos car
       ON car.id = uoc.id_cargo
-- Grado vigente (último asignado sin fecha fin).
LEFT JOIN persona.persona_grado pg
       ON pg.id = (SELECT MAX(pg2.id)
                     FROM persona.persona_grado pg2
                    WHERE pg2.id_persona = p.id
                      AND pg2.fecha_fin IS NULL)
LEFT JOIN estructura.grado gr
       ON gr.id = pg.id_grado
LEFT JOIN persona.sexo sx
       ON sx.id = p.id_sexo;

-- ---------------------------------------------------------------------
-- 2. VISTA DETALLE PLAN GUARDIA
--    Antes: solo id_persona.
--    Ahora: + datos completos de la persona asignada (incluye sexo)
--           y nombres de las personas de auditoría (crea / actualiza).
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vista_detalle_plan_guardia;

CREATE VIEW vista_detalle_plan_guardia AS
SELECT
    dpg.id_detalle_plan               AS id_detalle_plan,
    dpg.id_plan_guardia               AS id_plan_guardia,
    pg.fecha_inicial                  AS plan_fecha_inicial,
    pg.fecha_fin                      AS plan_fecha_fin,
    dpg.fecha_guardia                 AS fecha_guardia,
    dpg.hora_inicio                   AS hora_inicio,
    dpg.hora_fin                      AS hora_fin,
    dpg.observacion                   AS observacion,
    dpg.id_grupo_guardia              AS id_grupo_guardia,
    gg.nombre                         AS grupo_nombre,
    gg.iniciales                      AS grupo_iniciales,
    dpg.id_persona                    AS id_persona,
    per.nombre                        AS nombre,
    per.apellidos                     AS apellidos,
    CONCAT_WS(' ', per.nombre, per.apellidos) AS persona_nombre_completo,
    per.carnet_identidad              AS carnet_identidad,
    uoc.id_cargo AS id_cargo,
    car.descripcion                   AS cargo_descripcion,
    pgr.id_grado                      AS id_grado,
    gr.descripcion                    AS grado_descripcion,
    COALESCE(sx.nombre, sx.descripcion) AS sexo,
    dpg.id_persona_crea               AS id_persona_crea,
    CONCAT_WS(' ', pcrea.nombre, pcrea.apellidos) AS persona_crea_nombre,
    dpg.id_persona_actualiza          AS id_persona_actualiza,
    CONCAT_WS(' ', pact.nombre, pact.apellidos) AS persona_actualiza_nombre,
    dpg.fecha_hora_creado             AS fecha_hora_creado,
    dpg.fecha_hora_actualizado        AS fecha_hora_actualizado,
    CASE WHEN EXISTS (
             SELECT 1
             FROM guardia.vista_personas_afectaciones vpa
             WHERE vpa.id_persona = dpg.id_persona
               AND vpa.id_afectacion IS NOT NULL
               AND CAST(dpg.fecha_guardia AS DATETIME)
                       BETWEEN vpa.fecha_hora_inicio
                           AND COALESCE(vpa.fecha_hora_fin, '9999-12-31 23:59:59')
         ) THEN 1 ELSE 0 END          AS tiene_afectacion_fuerzas_medios
FROM guardia.detalle_plan_guardia dpg
INNER JOIN guardia.plan_guardia pg
        ON pg.id_plan_guardia = dpg.id_plan_guardia
INNER JOIN guardia.grupo_guardia gg
        ON gg.id_grupo_guardia = dpg.id_grupo_guardia
LEFT JOIN persona.persona per
       ON per.id = dpg.id_persona
LEFT JOIN persona.persona_cargo pc
       ON pc.id = (SELECT MAX(pc2.id)
                     FROM persona.persona_cargo pc2
                    WHERE pc2.id_persona = per.id
                      AND pc2.fecha_fin_cargo IS NULL)
LEFT JOIN estructura.unidad_organizativa_cargos uoc
       ON uoc.id = pc.id_cargo_unidad_organizativa
LEFT JOIN estructura.cargos car
       ON car.id = uoc.id_cargo
LEFT JOIN persona.persona_grado pgr
       ON pgr.id = (SELECT MAX(pgr2.id)
                      FROM persona.persona_grado pgr2
                     WHERE pgr2.id_persona = per.id
                       AND pgr2.fecha_fin IS NULL)
LEFT JOIN estructura.grado gr
       ON gr.id = pgr.id_grado
LEFT JOIN persona.sexo sx
       ON sx.id = per.id_sexo
LEFT JOIN persona.persona pcrea
       ON pcrea.id = dpg.id_persona_crea
LEFT JOIN persona.persona pact
       ON pact.id = dpg.id_persona_actualiza;

-- ---------------------------------------------------------------------
-- 3. VISTA RESPONSABLE APROBACIÓN GUARDIA
--    Datos de la persona responsable (nombre completo, carné, sexo)
--    en lugar de solo el id. La unidad organizativa vive en el esquema
--    `estructura` (no existe persona.unidad_organizativa).
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vista_responsable_aprobacion_guardia;

CREATE VIEW vista_responsable_aprobacion_guardia AS
SELECT
    rag.id_responsable                AS id_responsable,
    rag.id_unidad_organizativa        AS id_unidad_organizativa,
    COALESCE(NULLIF(uo.nombre, ''), uo.descripcion) AS unidad_organizativa_nombre,
    uo.descripcion                    AS unidad_organizativa_descripcion,
    rag.tipo                          AS tipo,
    rag.id_persona                    AS id_persona,
    p.nombre                          AS nombre,
    p.apellidos                       AS apellidos,
    CONCAT_WS(' ', p.nombre, p.apellidos) AS persona_nombre_completo,
    p.carnet_identidad                AS carnet_identidad,
    COALESCE(sx.nombre, sx.descripcion) AS sexo,
    uoc.id_cargo AS id_cargo,
    car.descripcion                   AS cargo_descripcion,
    pgr.id_grado                      AS id_grado,
    gr.descripcion                    AS grado_descripcion,
    rag.id_persona_crea               AS id_persona_crea,
    CONCAT_WS(' ', pcrea.nombre, pcrea.apellidos) AS persona_crea_nombre,
    rag.fecha_hora_creado             AS fecha_hora_creado,
    rag.fecha_hora_actualizado        AS fecha_hora_actualizado
FROM guardia.responsable_aprobacion_guardia rag
LEFT JOIN estructura.unidad_organizativa uo
       ON uo.id = rag.id_unidad_organizativa
LEFT JOIN persona.persona p
       ON p.id = rag.id_persona
LEFT JOIN persona.sexo sx
       ON sx.id = p.id_sexo
LEFT JOIN persona.persona_cargo pc
       ON pc.id = (SELECT MAX(pc2.id)
                     FROM persona.persona_cargo pc2
                    WHERE pc2.id_persona = p.id
                      AND pc2.fecha_fin_cargo IS NULL)
LEFT JOIN estructura.unidad_organizativa_cargos uoc
       ON uoc.id = pc.id_cargo_unidad_organizativa
LEFT JOIN estructura.cargos car
       ON car.id = uoc.id_cargo
LEFT JOIN persona.persona_grado pgr
       ON pgr.id = (SELECT MAX(pgr2.id)
                      FROM persona.persona_grado pgr2
                     WHERE pgr2.id_persona = p.id
                       AND pgr2.fecha_fin IS NULL)
LEFT JOIN estructura.grado gr
       ON gr.id = pgr.id_grado
LEFT JOIN persona.persona pcrea
       ON pcrea.id = rag.id_persona_crea;

-- ---------------------------------------------------------------------
-- 4. VISTA PERSONAS UNIDADES SUBORDINACIÓN
--    Personas con su unidad organizativa (derivada del cargo vigente),
--    su cargo, su grado y su SEXO. Necesaria para la planificación
--    automática y para el catálogo de personas disponibles.
--    NOTA: solo aparecen personas con cargo vigente (unidad conocida).
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vista_personas_unidades_subordinacion;

CREATE VIEW vista_personas_unidades_subordinacion AS
SELECT
    p.id                              AS id_persona,
    p.nombre                          AS nombre,
    p.apellidos                       AS apellidos,
    CONCAT_WS(' ', p.nombre, p.apellidos) AS nombre_completo,
    p.carnet_identidad                AS carnet_identidad,
    uoc.id_unidad_organizativa        AS id_unidad_organizativa,
    uo.nombre                         AS unidad_organizativa_nombre,
    uo.descripcion                    AS unidad_organizativa_descripcion,
    uoc.id_cargo AS id_cargo,
    car.descripcion                   AS cargo_descripcion,
    pgr.id_grado                      AS id_grado,
    gr.descripcion                    AS grado_descripcion,
    COALESCE(sx.nombre, sx.descripcion) AS sexo,
    anc.IdsUnidadesSuperiores         AS ids_unidades_superiores,
    GROUP_CONCAT(DISTINCT auo.nombre ORDER BY auo.nombre SEPARATOR ' | ')
                                      AS unidades_superiores,
    GROUP_CONCAT(DISTINCT auo.descripcion ORDER BY auo.descripcion SEPARATOR ' | ')
                                      AS unidades_superiores_descripcion
FROM persona.persona p
INNER JOIN persona.persona_cargo pc
       ON pc.id = (SELECT MAX(pc2.id)
                     FROM persona.persona_cargo pc2
                    WHERE pc2.id_persona = p.id
                      AND pc2.fecha_fin_cargo IS NULL)
INNER JOIN estructura.unidad_organizativa_cargos uoc
       ON uoc.id = pc.id_cargo_unidad_organizativa
INNER JOIN estructura.unidad_organizativa uo
       ON uo.id = uoc.id_unidad_organizativa
LEFT JOIN estructura.cargos car
       ON car.id = uoc.id_cargo
LEFT JOIN persona.persona_grado pgr
       ON pgr.id = (SELECT MAX(pgr2.id)
                      FROM persona.persona_grado pgr2
                     WHERE pgr2.id_persona = p.id
                       AND pgr2.fecha_fin IS NULL)
LEFT JOIN estructura.grado gr
       ON gr.id = pgr.id_grado
LEFT JOIN persona.sexo sx
       ON sx.id = p.id_sexo
-- Ancestros de la unidad (para el filtro de subordinación).
LEFT JOIN guardia.vista_arbol_unidades_organizativas anc
       ON anc.IdUnidadOrganizativa = uoc.id_unidad_organizativa
LEFT JOIN estructura.unidad_organizativa auo
       ON FIND_IN_SET(auo.id, COALESCE(anc.IdsUnidadesSuperiores, '')) > 0
GROUP BY
    p.id, p.nombre, p.apellidos, p.carnet_identidad,
    uoc.id_unidad_organizativa, uo.nombre, uo.descripcion,
    pc.id_cargo_unidad_organizativa, car.descripcion,
    pgr.id_grado, gr.descripcion, sx.nombre, sx.descripcion;

-- =====================================================================
-- NOTA: si la vista `vista_arbol_unidades_organizativas` de su servidor
-- usa nombres de columna distintos (p. ej. id_unidad_organizativa /
-- ids_unidades_superiores en minúsculas), ajuste los alias `anc.*`
-- de arriba. La columna imprescindible para el backend es `sexo`
-- ('Masculino' / 'Femenino'), presente en:
--   - vista_grupo_guardia_persona
--   - vista_detalle_plan_guardia
--   - vista_responsable_aprobacion_guardia
--   - vista_personas_unidades_subordinacion
-- =====================================================================
