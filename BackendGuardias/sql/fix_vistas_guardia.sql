-- =====================================================================
-- FIX VISTAS DEL ESQUEMA `guardia`
--
-- Objetivo: que las vistas del módulo de guardias NO devuelvan solo el
-- id_persona, sino los DATOS COMPLETOS de la persona (nombre, apellidos,
-- nombre completo, carné de identidad, cargo, grado y SEXO), obtenidos
-- mediante JOIN con la tabla `persona`.`persona`.
--
-- IMPORTANTE: este script NO modifica en absoluto la base `persona` ni
-- su tabla `persona`. Solo reescribe vistas del esquema `guardia`.
--
-- Suposiciones (según el modelo EF Core del backend):
--   * Las tablas de `guardia` almacenan id_persona, id_cargo, id_grado...
--     como columnas sueltas (sin FK hacia la base `persona`).
--   * La vista `vista_personas_afectaciones` proviene del sistema de
--     personas/afectaciones y NO se toca aquí.
--   * Los nombres de columnas de las vistas coinciden con las
--     convenciones snake_case usadas por GuardiasDbContext.
--
-- Ejecutar contra el servidor MySQL configurado en .env
-- (Server=192.168.4.33;Port=3306;Database=guardia).
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
    p.id_cargo                        AS id_cargo,
    c.descripcion                     AS cargo_descripcion,
    p.id_grado                        AS id_grado,
    g.descripcion                     AS grado_descripcion,
    s.nombre                          AS sexo,
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
       ON p.id_persona = ggp.id_persona
LEFT JOIN persona.cargo c
       ON c.id_cargo = p.id_cargo
LEFT JOIN persona.grado g
       ON g.id_grado = p.id_grado
LEFT JOIN persona.sexo s
       ON s.id_sexo = p.id_sexo;

-- ---------------------------------------------------------------------
-- 2. VISTA DETALLE PLAN GUARDIA
--    Antes: solo id_persona.
--    Ahora: + datos completos de la persona asignada y de las personas
--           de auditoría (crea / actualiza), incluyendo el sexo.
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
    p.nombre                          AS nombre,
    p.apellidos                       AS apellidos,
    CONCAT_WS(' ', p.nombre, p.apellidos) AS persona_nombre_completo,
    p.carnet_identidad                AS carnet_identidad,
    p.id_cargo                        AS id_cargo,
    c.descripcion                     AS cargo_descripcion,
    p.id_grado                        AS id_grado,
    g.descripcion                     AS grado_descripcion,
    s.nombre                          AS sexo,
    dpg.id_persona_crea               AS id_persona_crea,
    CONCAT_WS(' ', pc.nombre, pc.apellidos) AS persona_crea_nombre,
    dpg.id_persona_actualiza          AS id_persona_actualiza,
    CONCAT_WS(' ', pu.nombre, pu.apellidos) AS persona_actualiza_nombre,
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
LEFT JOIN persona.persona p
       ON p.id_persona = dpg.id_persona
LEFT JOIN persona.cargo c
       ON c.id_cargo = p.id_cargo
LEFT JOIN persona.grado g
       ON g.id_grado = p.id_grado
LEFT JOIN persona.sexo s
       ON s.id_sexo = p.id_sexo
LEFT JOIN persona.persona pc
       ON pc.id_persona = dpg.id_persona_crea
LEFT JOIN persona.persona pu
       ON pu.id_persona = dpg.id_persona_actualiza;

-- ---------------------------------------------------------------------
-- 3. VISTA RESPONSABLE APROBACIÓN GUARDIA
--    Igual que antes + datos de la persona responsable (nombre completo,
--    carnet y sexo) en lugar de solo el id.
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vista_responsable_aprobacion_guardia;

CREATE VIEW vista_responsable_aprobacion_guardia AS
SELECT
    rag.id_responsable                AS id_responsable,
    rag.id_unidad_organizativa        AS id_unidad_organizativa,
    uo.nombre                         AS unidad_organizativa_nombre,
    uo.descripcion                    AS unidad_organizativa_descripcion,
    rag.tipo                          AS tipo,
    rag.id_persona                    AS id_persona,
    p.nombre                          AS nombre,
    p.apellidos                       AS apellidos,
    CONCAT_WS(' ', p.nombre, p.apellidos) AS persona_nombre_completo,
    p.carnet_identidad                AS carnet_identidad,
    s.nombre                          AS sexo,
    rag.fecha_hora_creado             AS fecha_hora_creado,
    rag.fecha_hora_actualizado        AS fecha_hora_actualizado
FROM guardia.responsable_aprobacion_guardia rag
LEFT JOIN persona.unidad_organizativa uo
       ON uo.id_unidad_organizativa = rag.id_unidad_organizativa
LEFT JOIN persona.persona p
       ON p.id_persona = rag.id_persona
LEFT JOIN persona.sexo s
       ON s.id_sexo = p.id_sexo;

-- ---------------------------------------------------------------------
-- 4. VISTA PERSONAS UNIDADES SUBORDINACIÓN
--    Se mantiene igual que la definición actual del backend pero
--    añadiendo la columna `sexo`, necesaria para que la planificación
--    automática pueda excluir a las mujeres de los fines de semana.
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vista_personas_unidades_subordinacion;

CREATE VIEW vista_personas_unidades_subordinacion AS
SELECT
    p.id_persona                      AS id_persona,
    p.nombre                          AS nombre,
    p.apellidos                       AS apellidos,
    CONCAT_WS(' ', p.nombre, p.apellidos) AS nombre_completo,
    p.carnet_identidad                AS carnet_identidad,
    p.id_unidad_organizativa          AS id_unidad_organizativa,
    uo.nombre                         AS unidad_organizativa_nombre,
    uo.descripcion                    AS unidad_organizativa_descripcion,
    p.id_cargo                        AS id_cargo,
    c.descripcion                     AS cargo_descripcion,
    p.id_grado                        AS id_grado,
    g.descripcion                     AS grado_descripcion,
    s.nombre                          AS sexo,
    uo.ids_unidades_superiores        AS ids_unidades_superiores,
    uo.unidades_superiores            AS unidades_superiores,
    uo.unidades_superiores_descripcion AS unidades_superiores_descripcion
FROM persona.persona p
LEFT JOIN persona.unidad_organizativa uo
       ON uo.id_unidad_organizativa = p.id_unidad_organizativa
LEFT JOIN persona.cargo c
       ON c.id_cargo = p.id_cargo
LEFT JOIN persona.grado g
       ON g.id_grado = p.id_grado
LEFT JOIN persona.sexo s
       ON s.id_sexo = p.id_sexo;

-- =====================================================================
-- NOTA: si en su servidor las tablas auxiliares tienen otros nombres
-- (por ejemplo `persona.tabla_sexo`, `persona.cargos`...), ajuste los
-- JOIN de arriba. La única columna imprescindible para el backend es
-- `sexo` (nombre del sexo, p. ej. 'Masculino' / 'Femenino'), que debe
-- estar presente en:
--   - vista_grupo_guardia_persona
--   - vista_detalle_plan_guardia
--   - vista_personas_unidades_subordinacion
-- =====================================================================
