namespace DatabaseScaffold.Core.Motors
{
    using DatabaseScaffold.Interfaces;
    using DatabaseScaffold.Motors;
    using System;

    public class MotorOptionBase : NotifyObject, IMotorOption
    {
        private bool apply;
        private string @params;

        public MotorOptionBase(string parameter, string name, string description, bool hasParams)
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            HasParams = hasParams;
        }

        public string Parameter { get; }
        public string Name { get; }
        public string Description { get; }
        public bool HasParams { get; }
        public bool Apply { get => apply; set { apply = value; OnPropertyChanged(); OnPropertyChanged("HasParamsAndSelected"); } }
        public string Params { get => @params; set { @params = value; OnPropertyChanged(); } }

        public bool HasParamsAndSelected => HasParams && Apply;
    }
}
