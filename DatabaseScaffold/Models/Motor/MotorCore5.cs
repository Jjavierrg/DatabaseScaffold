namespace DatabaseScaffold.Models
{
    using System.Linq;

    public class MotorCore5 : BaseMotor
    {
        public MotorCore5() : base()
        {
            Options = Options.Concat(new[] {
                new MotorOptionBase("context-namespace", "Context Namespace", "Espacio de nombres que se va a utilizar para la clase generada DbContext . Nota: invalida Namespace ", true),
                new MotorOptionBase("namespace", "Namespace", "Espacio de nombres que se va a usar para todas las clases generadas. De forma predeterminada, se genera a partir del espacio de nombres raíz y el directorio de salida", true),
                new MotorOptionBase("no-onconfiguring", "No Onconfiguring", "Suprime la generación del OnConfiguring método en la clase generada DbContext", false),
                new MotorOptionBase("no-pluralize", "No Pluralize", "No use pluralizador", false),
            });
        }

        public override string Name => "Motor v5";
    }
}
