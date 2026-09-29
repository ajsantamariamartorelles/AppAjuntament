namespace AppAjuntament.Services.Cursets
{
    /// <summary>
    /// Dades de la persona interessada imprescindibles per poder emetre una
    /// autoliquidació (secció 2 de la plantilla): NIF/DNI + adreça completa.
    /// El nom i cognoms ja és obligatori a tot el mòdul.
    /// </summary>
    public static class LiquidacioDades
    {
        public static List<string> QueFalten(string? dni, string? adreca, string? codiPostal, string? poblacio)
        {
            var falten = new List<string>();
            if (string.IsNullOrWhiteSpace(dni)) falten.Add("NIF/DNI");
            if (string.IsNullOrWhiteSpace(adreca)) falten.Add("adreça");
            if (string.IsNullOrWhiteSpace(codiPostal)) falten.Add("codi postal");
            if (string.IsNullOrWhiteSpace(poblacio)) falten.Add("població");
            return falten;
        }
    }
}
