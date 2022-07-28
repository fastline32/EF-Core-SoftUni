namespace Artillery.DataProcessor.ExportDto
{
    public class ShellExportModel
    {
        public double ShellWeight { get; set; }
        public string Caliber { get; set; }

        public GunsExportModel[] Guns { get; set; }
    }

    public class GunsExportModel
    {
        public string GunType { get; set; }
        public int GunWeight { get; set; }
        public double BarrelLength { get; set; }
        public string Range { get; set; }
    }
}