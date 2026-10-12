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


-- Vista de árbol: IDs para relaciones y etiquetas para mostrar.
CREATE OR REPLACE ALGORITHM = UNDEFINED SQL SECURITY INVOKER
VIEW vista_arbol_unidades_organizativas AS
SELECT
    u.id AS id_unidad_organizativa,
    u.nombre AS nombre,
    u.descripcion AS descripcion,
    u.id_categoria AS id_categoria,
    cat.nombre AS categoria_nombre,
    cat.descripcion AS categoria_descripcion,
    rel.ids_unidades_superiores AS ids_unidades_superiores,
    rel.unidades_superiores AS unidades_superiores,
    rel.unidades_superiores_descripcion AS unidades_superiores_descripcion
FROM estructura.unidad_organizativa u
LEFT JOIN estructura.unidad_organizativa_categoria cat
    ON cat.id = u.id_categoria
LEFT JOIN (
    SELECT
        r.id_unidad_organizativa_menor,
        GROUP_CONCAT(DISTINCT CAST(r.id_unidad_organizativa_mayor AS CHAR)
            ORDER BY r.id_unidad_organizativa_mayor SEPARATOR ',') AS ids_unidades_superiores,
        GROUP_CONCAT(DISTINCT CONCAT(r.id_unidad_organizativa_mayor, ': ', um.nombre)
            ORDER BY r.id_unidad_organizativa_mayor SEPARATOR ' | ') AS unidades_superiores,
        GROUP_CONCAT(DISTINCT CONCAT(r.id_unidad_organizativa_mayor, ': ', COALESCE(um.descripcion, ''))
            ORDER BY r.id_unidad_organizativa_mayor SEPARATOR ' | ') AS unidades_superiores_descripcion
    FROM estructura.unidad_organizativa_relacion r
    INNER JOIN estructura.unidad_organizativa um
        ON um.id = r.id_unidad_organizativa_mayor
    GROUP BY r.id_unidad_organizativa_menor
) rel ON rel.id_unidad_organizativa_menor = u.id;

-- La lista de grupos devuelve padre, cantidad de personas y auditoría legible.
CREATE OR REPLACE ALGORITHM = UNDEFINED SQL SECURITY INVOKER
VIEW vista_grupos_guardia AS
SELECT
    gg.id_grupo_guardia AS id_grupo_guardia,
    gg.nombre AS grupo_nombre,
    gg.iniciales AS iniciales,
    gg.subordinado_a_id_grupo_guardia AS subordinado_a_id_grupo_guardia,
    padre.nombre AS grupo_padre_nombre,
    padre.iniciales AS grupo_padre_iniciales,
    gg.color AS color,
    gg.icono AS icono,
    gg.observacion AS observacion,
    gg.activo AS activo,
    COALESCE(personas.cantidad_personas, 0) AS cantidad_personas,
    gg.id_persona_crea AS id_persona_crea,
    TRIM(CONCAT_WS(' ', pcrea.nombre, pcrea.apellidos)) AS persona_crea_nombre,
    gg.id_persona_actualiza AS id_persona_actualiza,
    TRIM(CONCAT_WS(' ', pact.nombre, pact.apellidos)) AS persona_actualiza_nombre,
    gg.fecha_hora_creado AS fecha_hora_creado,
    gg.fecha_hora_actualizado AS fecha_hora_actualizado
FROM guardia.grupo_guardia gg
LEFT JOIN guardia.grupo_guardia padre
    ON padre.id_grupo_guardia = gg.subordinado_a_id_grupo_guardia
LEFT JOIN (
    SELECT id_grupo_guardia, COUNT(DISTINCT id_persona) AS cantidad_personas
    FROM guardia.grupo_guardia_persona
    WHERE activo = 1
    GROUP BY id_grupo_guardia
) personas ON personas.id_grupo_guardia = gg.id_grupo_guardia
LEFT JOIN persona.persona pcrea
    ON pcrea.id = gg.id_persona_crea
LEFT JOIN persona.persona pact
    ON pact.id = gg.id_persona_actualiza;


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
    ggp.id_grupo_guardia_persona AS id_grupo_guardia_persona,
    ggp.id_grupo_guardia AS id_grupo_guardia,
    gg.nombre AS grupo_nombre,
    gg.iniciales AS grupo_iniciales,
    gg.color AS grupo_color,
    gg.icono AS grupo_icono,
    ggp.id_persona AS id_persona,
    p.nombre AS nombre,
    p.apellidos AS apellidos,
    TRIM(CONCAT_WS(' ', p.nombre, p.apellidos)) AS persona_nombre_completo,
    p.carnet_identidad AS carnet_identidad,
    p.id_sexo AS id_sexo,
    sx.nombre AS sexo_nombre,
    sx.descripcion AS sexo_descripcion,
    COALESCE(NULLIF(sx.nombre, ''), NULLIF(sx.descripcion, ''), 'No especificado') AS sexo,
    p.id_color_piel AS id_color_piel,
    cp.nombre AS color_piel_nombre,
    cp.descripcion AS color_piel_descripcion,
    pc.id_cargo_unidad_organizativa AS id_cargo_unidad_organizativa,
    uoc.id_cargo AS id_cargo,
    car.nombre AS cargo_nombre,
    car.descripcion AS cargo_descripcion,
    uoc.id_unidad_organizativa AS id_unidad_organizativa,
    uo.nombre AS unidad_organizativa_nombre,
    uo.descripcion AS unidad_organizativa_descripcion,
    ucat.id AS id_categoria_unidad_organizativa,
    ucat.nombre AS categoria_unidad_organizativa_nombre,
    ucat.descripcion AS categoria_unidad_organizativa_descripcion,
    pg.id_grado AS id_grado,
    gr.nombre AS grado_nombre,
    gr.descripcion AS grado_descripcion,
    COALESCE(NULLIF(gr.nombre, ''), NULLIF(gr.descripcion, ''), 'No especificado') AS grado,
    ggp.fecha_inicio AS fecha_inicio,
    ggp.fecha_fin AS fecha_fin,
    ggp.activo AS activo,
    ggp.observacion AS observacion,
    ggp.id_persona_crea AS id_persona_crea,
    TRIM(CONCAT_WS(' ', pcrea.nombre, pcrea.apellidos)) AS persona_crea_nombre,
    ggp.id_persona_actualiza AS id_persona_actualiza,
    TRIM(CONCAT_WS(' ', pact.nombre, pact.apellidos)) AS persona_actualiza_nombre,
    ggp.fecha_hora_creado AS fecha_hora_creado,
    ggp.fecha_hora_actualizado AS fecha_hora_actualizado
FROM guardia.grupo_guardia_persona ggp
INNER JOIN guardia.grupo_guardia gg
    ON gg.id_grupo_guardia = ggp.id_grupo_guardia
LEFT JOIN persona.persona p
    ON p.id = ggp.id_persona
LEFT JOIN persona.sexo sx
    ON sx.id = p.id_sexo
LEFT JOIN persona.color_piel cp
    ON cp.id = p.id_color_piel
LEFT JOIN persona.persona_cargo pc
    ON pc.id = (
        SELECT MAX(pc2.id)
        FROM persona.persona_cargo pc2
        WHERE pc2.id_persona = p.id
          AND pc2.fecha_fin_cargo IS NULL
    )
LEFT JOIN estructura.unidad_organizativa_cargos uoc
    ON uoc.id = pc.id_cargo_unidad_organizativa
LEFT JOIN estructura.cargos car
    ON car.id = uoc.id_cargo
LEFT JOIN estructura.unidad_organizativa uo
    ON uo.id = uoc.id_unidad_organizativa
LEFT JOIN estructura.unidad_organizativa_categoria ucat
    ON ucat.id = uo.id_categoria
LEFT JOIN persona.persona_grado pg
    ON pg.id = (
        SELECT MAX(pg2.id)
        FROM persona.persona_grado pg2
        WHERE pg2.id_persona = p.id
          AND pg2.fecha_fin IS NULL
    )
LEFT JOIN estructura.grado gr
    ON gr.id = pg.id_grado
LEFT JOIN persona.persona pcrea
    ON pcrea.id = ggp.id_persona_crea
LEFT JOIN persona.persona pact
    ON pact.id = ggp.id_persona_actualiza;

-- ---------------------------------------------------------------------
-- 2. VISTA DETALLE PLAN GUARDIA
--    Antes: solo id_persona.
--    Ahora: + datos completos de la persona asignada (incluye sexo)
--           y nombres de las personas de auditoría (crea / actualiza).
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vista_detalle_plan_guardia;

CREATE VIEW vista_detalle_plan_guardia AS
SELECT
    dpg.id_detalle_plan AS id_detalle_plan,
    dpg.id_plan_guardia AS id_plan_guardia,
    pg.fecha_inicial AS plan_fecha_inicial,
    pg.fecha_fin AS plan_fecha_fin,
    pg.id_estado AS id_estado,
    ep.codigo AS estado_codigo,
    ep.nombre AS estado_nombre,
    ep.descripcion AS estado_descripcion,
    ep.color AS estado_color,
    ep.icono AS estado_icono,
    dpg.fecha_guardia AS fecha_guardia,
    dpg.hora_inicio AS hora_inicio,
    dpg.hora_fin AS hora_fin,
    dpg.observacion AS observacion,
    dpg.id_grupo_guardia AS id_grupo_guardia,
    gg.nombre AS grupo_nombre,
    gg.iniciales AS grupo_iniciales,
    gg.color AS grupo_color,
    gg.icono AS grupo_icono,
    dpg.id_persona AS id_persona,
    per.nombre AS nombre,
    per.apellidos AS apellidos,
    TRIM(CONCAT_WS(' ', per.nombre, per.apellidos)) AS persona_nombre_completo,
    per.carnet_identidad AS carnet_identidad,
    per.id_sexo AS id_sexo,
    sx.nombre AS sexo_nombre,
    sx.descripcion AS sexo_descripcion,
    COALESCE(NULLIF(sx.nombre, ''), NULLIF(sx.descripcion, ''), 'No especificado') AS sexo,
    per.id_color_piel AS id_color_piel,
    cp.nombre AS color_piel_nombre,
    cp.descripcion AS color_piel_descripcion,
    pc.id_cargo_unidad_organizativa AS id_cargo_unidad_organizativa,
    uoc.id_cargo AS id_cargo,
    car.nombre AS cargo_nombre,
    car.descripcion AS cargo_descripcion,
    uoc.id_unidad_organizativa AS id_unidad_organizativa,
    uo.nombre AS unidad_organizativa_nombre,
    uo.descripcion AS unidad_organizativa_descripcion,
    ucat.id AS id_categoria_unidad_organizativa,
    ucat.nombre AS categoria_unidad_organizativa_nombre,
    ucat.descripcion AS categoria_unidad_organizativa_descripcion,
    pgr.id_grado AS id_grado,
    gr.nombre AS grado_nombre,
    gr.descripcion AS grado_descripcion,
    COALESCE(NULLIF(gr.nombre, ''), NULLIF(gr.descripcion, ''), 'No especificado') AS grado,
    dpg.id_persona_crea AS id_persona_crea,
    TRIM(CONCAT_WS(' ', pcrea.nombre, pcrea.apellidos)) AS persona_crea_nombre,
    dpg.id_persona_actualiza AS id_persona_actualiza,
    TRIM(CONCAT_WS(' ', pact.nombre, pact.apellidos)) AS persona_actualiza_nombre,
    dpg.fecha_hora_creado AS fecha_hora_creado,
    dpg.fecha_hora_actualizado AS fecha_hora_actualizado,
    CASE WHEN EXISTS (
        SELECT 1
        FROM guardia.vista_personas_afectaciones vpa
        WHERE vpa.id_persona = dpg.id_persona
          AND vpa.id_afectacion IS NOT NULL
          AND CAST(dpg.fecha_guardia AS DATETIME)
              BETWEEN vpa.fecha_hora_inicio
                  AND COALESCE(vpa.fecha_hora_fin, '9999-12-31 23:59:59')
    ) THEN 1 ELSE 0 END AS tiene_afectacion_fuerzas_medios
FROM guardia.detalle_plan_guardia dpg
INNER JOIN guardia.plan_guardia pg
    ON pg.id_plan_guardia = dpg.id_plan_guardia
LEFT JOIN guardia.estado_parte ep
    ON ep.id_estado = pg.id_estado
INNER JOIN guardia.grupo_guardia gg
    ON gg.id_grupo_guardia = dpg.id_grupo_guardia
LEFT JOIN persona.persona per
    ON per.id = dpg.id_persona
LEFT JOIN persona.sexo sx
    ON sx.id = per.id_sexo
LEFT JOIN persona.color_piel cp
    ON cp.id = per.id_color_piel
LEFT JOIN persona.persona_cargo pc
    ON pc.id = (
        SELECT MAX(pc2.id)
        FROM persona.persona_cargo pc2
        WHERE pc2.id_persona = per.id
          AND pc2.fecha_fin_cargo IS NULL
    )
LEFT JOIN estructura.unidad_organizativa_cargos uoc
    ON uoc.id = pc.id_cargo_unidad_organizativa
LEFT JOIN estructura.cargos car
    ON car.id = uoc.id_cargo
LEFT JOIN estructura.unidad_organizativa uo
    ON uo.id = uoc.id_unidad_organizativa
LEFT JOIN estructura.unidad_organizativa_categoria ucat
    ON ucat.id = uo.id_categoria
LEFT JOIN persona.persona_grado pgr
    ON pgr.id = (
        SELECT MAX(pgr2.id)
        FROM persona.persona_grado pgr2
        WHERE pgr2.id_persona = per.id
          AND pgr2.fecha_fin IS NULL
    )
LEFT JOIN estructura.grado gr
    ON gr.id = pgr.id_grado
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
    rag.id_responsable AS id_responsable,
    rag.id_unidad_organizativa AS id_unidad_organizativa,
    COALESCE(NULLIF(uor.nombre, ''), uor.descripcion) AS unidad_organizativa_nombre,
    uor.descripcion AS unidad_organizativa_descripcion,
    ucatr.id AS id_categoria_unidad_organizativa,
    ucatr.nombre AS categoria_unidad_organizativa_nombre,
    ucatr.descripcion AS categoria_unidad_organizativa_descripcion,
    rag.tipo AS tipo,
    rag.id_persona AS id_persona,
    p.nombre AS nombre,
    p.apellidos AS apellidos,
    TRIM(CONCAT_WS(' ', p.nombre, p.apellidos)) AS persona_nombre_completo,
    p.carnet_identidad AS carnet_identidad,
    p.id_sexo AS id_sexo,
    sx.nombre AS sexo_nombre,
    sx.descripcion AS sexo_descripcion,
    COALESCE(NULLIF(sx.nombre, ''), NULLIF(sx.descripcion, ''), 'No especificado') AS sexo,
    p.id_color_piel AS id_color_piel,
    cp.nombre AS color_piel_nombre,
    cp.descripcion AS color_piel_descripcion,
    pc.id_cargo_unidad_organizativa AS id_cargo_unidad_organizativa,
    uoc.id_cargo AS id_cargo,
    car.nombre AS cargo_nombre,
    car.descripcion AS cargo_descripcion,
    uoc.id_unidad_organizativa AS id_unidad_organizativa_persona,
    COALESCE(NULLIF(uop.nombre, ''), uop.descripcion) AS unidad_organizativa_persona_nombre,
    uop.descripcion AS unidad_organizativa_persona_descripcion,
    pgr.id_grado AS id_grado,
    gr.nombre AS grado_nombre,
    gr.descripcion AS grado_descripcion,
    COALESCE(NULLIF(gr.nombre, ''), NULLIF(gr.descripcion, ''), 'No especificado') AS grado,
    rag.id_persona_crea AS id_persona_crea,
    TRIM(CONCAT_WS(' ', pcrea.nombre, pcrea.apellidos)) AS persona_crea_nombre,
    rag.fecha_hora_creado AS fecha_hora_creado,
    rag.fecha_hora_actualizado AS fecha_hora_actualizado
FROM guardia.responsable_aprobacion_guardia rag
LEFT JOIN estructura.unidad_organizativa uor
    ON uor.id = rag.id_unidad_organizativa
LEFT JOIN estructura.unidad_organizativa_categoria ucatr
    ON ucatr.id = uor.id_categoria
LEFT JOIN persona.persona p
    ON p.id = rag.id_persona
LEFT JOIN persona.sexo sx
    ON sx.id = p.id_sexo
LEFT JOIN persona.color_piel cp
    ON cp.id = p.id_color_piel
LEFT JOIN persona.persona_cargo pc
    ON pc.id = (
        SELECT MAX(pc2.id)
        FROM persona.persona_cargo pc2
        WHERE pc2.id_persona = p.id
          AND pc2.fecha_fin_cargo IS NULL
    )
LEFT JOIN estructura.unidad_organizativa_cargos uoc
    ON uoc.id = pc.id_cargo_unidad_organizativa
LEFT JOIN estructura.cargos car
    ON car.id = uoc.id_cargo
LEFT JOIN estructura.unidad_organizativa uop
    ON uop.id = uoc.id_unidad_organizativa
LEFT JOIN persona.persona_grado pgr
    ON pgr.id = (
        SELECT MAX(pgr2.id)
        FROM persona.persona_grado pgr2
        WHERE pgr2.id_persona = p.id
          AND pgr2.fecha_fin IS NULL
    )
LEFT JOIN estructura.grado gr
    ON gr.id = pgr.id_grado
LEFT JOIN persona.persona pcrea
    ON pcrea.id = rag.id_persona_crea;

-- ---------------------------------------------------------------------
-- 4. VISTA PERSONAS UNIDADES SUBORDINACIÓN
--    Personas con su unidad organizativa (derivada del cargo vigente),
--    su cargo, su grado y su SEXO. Necesaria para la planificación
--    automática y para el
--    catálogo de personas disponibles.
--    NOTA: solo aparecen personas con cargo vigente (unidad conocida).
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vista_personas_unidades_subordinacion;

CREATE VIEW vista_personas_unidades_subordinacion AS
SELECT
    p.id AS id_persona,
    p.nombre AS nombre,
    p.apellidos AS apellidos,
    TRIM(CONCAT_WS(' ', p.nombre, p.apellidos)) AS nombre_completo,
    p.carnet_identidad AS carnet_identidad,
    p.id_sexo AS id_sexo,
    sx.nombre AS sexo_nombre,
    sx.descripcion AS sexo_descripcion,
    COALESCE(NULLIF(sx.nombre, ''), NULLIF(sx.descripcion, ''), 'No especificado') AS sexo,
    p.id_color_piel AS id_color_piel,
    cp.nombre AS color_piel_nombre,
    cp.descripcion AS color_piel_descripcion,
    pc.id AS id_persona_cargo,
    pc.id_cargo_unidad_organizativa AS id_cargo_unidad_organizativa,
    uoc.id_cargo AS id_cargo,
    car.nombre AS cargo_nombre,
    car.descripcion AS cargo_descripcion,
    pc.fecha_introduccion_cargo AS cargo_fecha_introduccion,
    pc.fecha_fin_cargo AS cargo_fecha_fin,
    pc.observaciones AS cargo_observacion,
    uo.id AS id_unidad_organizativa,
    uo.nombre AS unidad_organizativa_nombre,
    uo.descripcion AS unidad_organizativa_descripcion,
    ucat.id AS id_categoria_unidad_organizativa,
    ucat.nombre AS categoria_unidad_organizativa_nombre,
    ucat.descripcion AS categoria_unidad_organizativa_descripcion,
    pgr.id AS persona_grado_id,
    pgr.id_grado AS id_grado,
    gr.nombre AS grado_nombre,
    gr.descripcion AS grado_descripcion,
    pgr.fecha_insercion AS grado_fecha_insercion,
    pgr.fecha_fin AS grado_fecha_fin,
    pgr.observacion AS grado_observacion,
    p.id_persona_inserta AS id_persona_inserta,
    TRIM(CONCAT_WS(' ', pinserta.nombre, pinserta.apellidos)) AS persona_inserta_nombre,
    us.nombre AS nombre_usuario,
    CASE WHEN COALESCE(uni.cantidad_unidades_superiores, 0) > 0 THEN 1 ELSE 0 END AS tiene_unidad_superior,
    COALESCE(uni.cantidad_unidades_superiores, 0) AS cantidad_unidades_superiores,
    uni.ids_unidades_superiores AS ids_unidades_superiores,
    uni.unidades_superiores AS unidades_superiores,
    uni.unidades_superiores_descripcion AS unidades_superiores_descripcion
FROM persona.persona p
LEFT JOIN persona.sexo sx
    ON sx.id = p.id_sexo
LEFT JOIN persona.color_piel cp
    ON cp.id = p.id_color_piel
LEFT JOIN persona.persona_cargo pc
    ON pc.id = (
        SELECT MAX(pc2.id)
        FROM persona.persona_cargo pc2
        WHERE pc2.id_persona = p.id
          AND pc2.fecha_fin_cargo IS NULL
    )
LEFT JOIN estructura.unidad_organizativa_cargos uoc
    ON uoc.id = pc.id_cargo_unidad_organizativa
LEFT JOIN estructura.cargos car
    ON car.id = uoc.id_cargo
LEFT JOIN estructura.unidad_organizativa uo
    ON uo.id = uoc.id_unidad_organizativa
LEFT JOIN estructura.unidad_organizativa_categoria ucat
    ON ucat.id = uo.id_categoria
LEFT JOIN persona.persona_grado pgr
    ON pgr.id = (
        SELECT MAX(pg2.id)
        FROM persona.persona_grado pg2
        WHERE pg2.id_persona = p.id
          AND pg2.fecha_fin IS NULL
    )
LEFT JOIN estructura.grado gr
    ON gr.id = pgr.id_grado
LEFT JOIN persona.persona pinserta
    ON pinserta.id = p.id_persona_inserta
LEFT JOIN sistemas.usuarios us
    ON us.id_persona = p.id
LEFT JOIN (
    SELECT
        r.id_unidad_organizativa_menor,
        COUNT(DISTINCT r.id_unidad_organizativa_mayor) AS cantidad_unidades_superiores,
        GROUP_CONCAT(DISTINCT CAST(r.id_unidad_organizativa_mayor AS CHAR)
            ORDER BY r.id_unidad_organizativa_mayor SEPARATOR ',') AS ids_unidades_superiores,
        GROUP_CONCAT(DISTINCT CONCAT(r.id_unidad_organizativa_mayor, ': ', uom.nombre)
            ORDER BY r.id_unidad_organizativa_mayor SEPARATOR ' | ') AS unidades_superiores,
        GROUP_CONCAT(DISTINCT CONCAT(r.id_unidad_organizativa_mayor, ': ', COALESCE(uom.descripcion, ''))
            ORDER BY r.id_unidad_organizativa_mayor SEPARATOR ' | ') AS unidades_superiores_descripcion
    FROM estructura.unidad_organizativa_relacion r
    INNER JOIN estructura.unidad_organizativa uom
        ON uom.id = r.id_unidad_organizativa_mayor
    GROUP BY r.id_unidad_organizativa_menor
) uni ON uni.id_unidad_organizativa_menor = uo.id
GROUP BY
    p.id, p.nombre, p.apellidos, p.carnet_identidad, p.id_sexo, sx.nombre, sx.descripcion,
    p.id_color_piel, cp.nombre, cp.descripcion,
    pc.id, pc.id_cargo_unidad_organizativa, pc.fecha_introduccion_cargo, pc.fecha_fin_cargo,
    pc.observaciones, uoc.id_cargo, car.nombre, car.descripcion,
    uo.id, uo.nombre, uo.descripcion, ucat.id, ucat.nombre, ucat.descripcion,
    pgr.id, pgr.id_grado, gr.nombre, gr.descripcion, pgr.fecha_insercion, pgr.fecha_fin,
    pgr.observacion, p.id_persona_inserta, pinserta.nombre, pinserta.apellidos, us.nombre,
    uni.cantidad_unidades_superiores, uni.ids_unidades_superiores,
    uni.unidades_superiores, uni.unidades_superiores_descripcion;

-- =====================================================================
-- Vistas ampliadas: se preservan los IDs para escritura y se incluyen
-- nombres/descripciones para los DTO y la interfaz (sin mostrar IDs
-- como si fueran etiquetas).
-- =====================================================================
