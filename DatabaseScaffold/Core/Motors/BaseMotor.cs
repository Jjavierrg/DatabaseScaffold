namespace DatabaseScaffold.Core.Motors
{
    using DatabaseScaffold.Interfaces;
    using System.Collections.Generic;
    using System.Linq;

    public class BaseMotor : IMotor
    {
        public BaseMotor()
        {
            Options = new[] {
                new MotorOptionBase("d", "Data Annotations", "Use los atributos para configurar el modelo (siempre que sea posible). Si se omite esta opción, solo se usa la API fluida", false),
                new MotorOptionBase("c", "Context", "Nombre de la DbContext clase que se va a generar", true),
                new MotorOptionBase("context-dir", "Context Dir", "Directorio en el que se va a colocar el DbContext archivo de clase. Las rutas de acceso son relativas al directorio del proyecto. Los espacios de nombres se derivan de los nombres de carpeta.", true),
                new MotorOptionBase("f", "Force", "Sobrescribe los archivos existentes.", false),
                new MotorOptionBase("use-database-names", "Use Database Names", "Utilice nombres de tabla y columna exactamente como aparecen en la base de datos. Si se omite esta opción, se cambian los nombres de base de datos para que se ajusten mejor a las convenciones de estilo de nombre de C#.", false)
            };
        }

        public IEnumerable<IMotorOption> Options { get; set; }
        public virtual string Name => "Motor v3";

        public string GetParams()
        {
            var selectedOptions = Options.Where(x => x.Apply).Select(x => $"-{x.Parameter}{(x.HasParams ? " " + x.Params : "")}".Trim());
            if (selectedOptions?.Any() ?? false)
                return string.Join(" ", selectedOptions);
            else
                return string.Empty;
        }
    }
}
