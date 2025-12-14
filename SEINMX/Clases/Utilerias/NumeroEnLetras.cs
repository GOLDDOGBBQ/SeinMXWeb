namespace SEINMX.Clases.Utilerias;

using System;
using System.Globalization;
using System.Text;

public static class NumeroEnLetras
{
    public static string NumeroALetras(decimal numero)
    {
        long parteEntera = (long)Math.Floor(numero);
        int centavos = (int)Math.Round((numero - parteEntera) * 100);

        string letras = ConvertirNumero(parteEntera);

        // Pesos / peso
        letras += parteEntera == 1 ? " peso" : " pesos";

        if (centavos > 0)
        {
            letras += $" con {centavos:00}/100 M.N.";
        }

        // Capitaliza primera letra
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(letras);
    }

    private static string ConvertirNumero(long numero)
    {
        if (numero == 0)
            return "cero";

        if (numero < 0)
            return "menos " + ConvertirNumero(Math.Abs(numero));

        var sb = new StringBuilder();

        if (numero >= 1_000_000)
        {
            sb.Append(ConvertirNumero(numero / 1_000_000));
            sb.Append(numero / 1_000_000 == 1 ? " millón " : " millones ");
            numero %= 1_000_000;
        }

        if (numero >= 1000)
        {
            sb.Append(ConvertirNumero(numero / 1000));
            sb.Append(" mil ");
            numero %= 1000;
        }

        if (numero >= 100)
        {
            if (numero == 100)
            {
                sb.Append("cien");
                numero = 0;
            }
            else
            {
                sb.Append(CENTENAS[numero / 100]);
                numero %= 100;
            }
        }

        if (numero > 0)
            sb.Append(DECENAS[numero]);

        return sb.ToString().Trim();
    }

    private static readonly string[] DECENAS =
    {
        "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve",
        "diez", "once", "doce", "trece", "catorce", "quince",
        "dieciséis", "diecisiete", "dieciocho", "diecinueve",
        "veinte", "veintiuno", "veintidós", "veintitrés", "veinticuatro",
        "veinticinco", "veintiséis", "veintisiete", "veintiocho", "veintinueve",
        "treinta", "treinta y uno", "treinta y dos", "treinta y tres", "treinta y cuatro",
        "treinta y cinco", "treinta y seis", "treinta y siete", "treinta y ocho", "treinta y nueve",
        "cuarenta", "cuarenta y uno", "cuarenta y dos", "cuarenta y tres", "cuarenta y cuatro",
        "cuarenta y cinco", "cuarenta y seis", "cuarenta y siete", "cuarenta y ocho", "cuarenta y nueve",
        "cincuenta", "cincuenta y uno", "cincuenta y dos", "cincuenta y tres", "cincuenta y cuatro",
        "cincuenta y cinco", "cincuenta y seis", "cincuenta y siete", "cincuenta y ocho", "cincuenta y nueve",
        "sesenta", "sesenta y uno", "sesenta y dos", "sesenta y tres", "sesenta y cuatro",
        "sesenta y cinco", "sesenta y seis", "sesenta y siete", "sesenta y ocho", "sesenta y nueve",
        "setenta", "setenta y uno", "setenta y dos", "setenta y tres", "setenta y cuatro",
        "setenta y cinco", "setenta y seis", "setenta y siete", "setenta y ocho", "setenta y nueve",
        "ochenta", "ochenta y uno", "ochenta y dos", "ochenta y tres", "ochenta y cuatro",
        "ochenta y cinco", "ochenta y seis", "ochenta y siete", "ochenta y ocho", "ochenta y nueve",
        "noventa", "noventa y uno", "noventa y dos", "noventa y tres", "noventa y cuatro",
        "noventa y cinco", "noventa y seis", "noventa y siete", "noventa y ocho", "noventa y nueve"
    };

    private static readonly string[] CENTENAS =
    {
        "", "ciento ", "doscientos ", "trescientos ", "cuatrocientos ",
        "quinientos ", "seiscientos ", "setecientos ", "ochocientos ", "novecientos "
    };
}
