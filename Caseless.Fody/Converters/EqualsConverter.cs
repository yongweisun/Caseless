using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class EqualsConverter : IEqualityConverter
{
    // only if UseOperatorForOrdinal is true, then it means we are forcing a.Equals(b) being converted into a == b
    public bool? UseOperatorForOrdinal { get; set; }
    MethodReference reference;
    public ModuleWeaver ModuleWeaver { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public int StringComparisonConstant { get; set; }

    public void Init()
    {
        var methods = ModuleWeaver.StringDefinition.Methods;
        if (UseOperatorForOrdinal.GetValueOrDefault())
        {
            reference = ModuleDefinition.ImportReference(methods.First(x => x.Name == "op_Equality" && x.Parameters.Matches("String", "String")));
        }
        else
        {
            reference = ModuleDefinition.ImportReference(methods.First(x => x.Name == "Equals" && x.Parameters.Matches("String", "StringComparison")));
        }
    }

    public IEnumerable<Instruction> Convert(MethodReference method)
    {
        if (method.Name != "Equals")
        {
            yield break;
        }

        if (!method.Parameters.Matches("String"))
        {
            yield break;
        }

        if (UseOperatorForOrdinal.GetValueOrDefault())
        {
            yield return Instruction.Create(OpCodes.Call, reference);
        }
        else
        {
            yield return Instruction.Create(OpCodes.Ldc_I4, StringComparisonConstant);
            yield return Instruction.Create(OpCodes.Callvirt, reference);
        }
    }
}