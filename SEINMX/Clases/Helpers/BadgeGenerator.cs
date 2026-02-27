namespace SEINMX.Clases.Helpers;

public static class BadgeGenerator
{
    public static string BadgeColor(int? index)
    {
        switch (index)
        {
            case 1:
                return "badge bg-primary";

            case 2:
                return "badge bg-info";

            case 3:
                return "badge bg-success";

            case 4:
                return "badge bg-warning text-dark";

            case 5:
                return "badge bg-secondary";

            case 6:
                return "badge bg-dark";

            case 7:
                return "badge bg-danger";

            case 99:
                return "badge bg-danger";

            default:
                return "badge bg-light text-dark";
        }
    }

    public static string Badge(int? indexColor, string sTexto)
    {
        return $"<span class=\"{BadgeColor(indexColor)}\">{sTexto}</span>";
    }



    
}