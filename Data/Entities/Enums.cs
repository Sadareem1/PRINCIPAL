namespace SGI_INVENTARIO.Data.Entities;

public enum EstadoActivo
{
    Operativo = 0,
    Mantenimiento = 1,
    Baja = 2,
    Asignado = 3
}

public enum TipoMantenimiento
{
    Preventivo = 0,
    Correctivo = 1
}

public enum EstadoMantenimiento
{
    Programado = 0,
    EnProceso = 1,
    Completado = 2,
    Cancelado = 3
}

public enum Prioridad
{
    Baja = 0,
    Media = 1,
    Alta = 2
}

public enum EstadoSolicitud
{
    Abierta = 0,
    EnProceso = 1,
    Resuelta = 2,
    Cerrada = 3
}

public enum TipoSolicitud
{
    Equipo = 0,
    Mantenimiento = 1,
    Reemplazo = 2
}

public enum TipoAlerta
{
    Licencia = 0,
    Garantia = 1,
    Mantenimiento = 2,
    Contrato = 3
}

public enum CriticidadAlerta
{
    Baja = 0,
    Media = 1,
    Alta = 2
}

public enum TipoContrato
{
    Licencia = 0,
    Garantia = 1,
    Mantenimiento = 2,
    Otro = 3
}
