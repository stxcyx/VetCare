namespace VetCare.Abstracciones.Comunes;

public enum TipoResultado { Exito, NoEncontrado, Conflicto }

// Respuesta estándar de Flujo: dice si la operación salió bien y, si no, por qué.
public class Resultado<T>
{
    public TipoResultado Tipo { get; init; }
    public T? Datos { get; init; }
    public string? Campo { get; init; }
    public string? Mensaje { get; init; }

    public bool EsExito => Tipo == TipoResultado.Exito;

    public static Resultado<T> Exito(T datos) =>
        new() { Tipo = TipoResultado.Exito, Datos = datos };

    public static Resultado<T> NoEncontrado(string mensaje = "Registro no encontrado") =>
        new() { Tipo = TipoResultado.NoEncontrado, Mensaje = mensaje };

    public static Resultado<T> Conflicto(string mensaje, string? campo = null) =>
        new() { Tipo = TipoResultado.Conflicto, Mensaje = mensaje, Campo = campo };
}