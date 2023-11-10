ofile = open("./parfor_dump.cs", "w")

cardinals = ["one", "two", "three", "four", "five", "six", "seven", "eight",
             "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen"]
ordinals = ["first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth",
            "ninth", "tenth", "eleventh", "twelfth", "thirteenth", "fourteenth", "fifteenth", "sixteenth"]

letters = ["T", "U", "V", "W", "X", "Y", "Z",
           "A", "B", "C", "D", "E", "F", "G", "H", "I"]

for i in range(0, 16):
    funcstr = ""

    funcstr += """/// <summary>
/// Creates a GPU parallel for loop.
/// The body of the kernel is run on a GPU target.
/// This overload specifies that {c} arrays are used on the GPU.
/// </summary>
/// <param name="start">The start of the loop, inclusive.</param>
/// <param name="end">The end of the loop, exclusive.</param>""".format(c=cardinals[i])

    for j in range(i + 1):
        adjusted = j + 1

        funcstr += """
/// <param name="buf{a}">The {o} buffer to run the kernel with.</param>""".format(a=j + 1, o=ordinals[j])

    funcstr += """
/// <param name="action">The kernel to run on the GPU.</param>"""

    for j in range(i + 1):
        funcstr += """
/// <typeparam name="{l}">The base type of the {o} argument. Must be an unmanaged type.</typeparam>""".format(l=letters[j], o=ordinals[j])

    funcstr += """
public static void ParallelFor<"""

    for j in range(i):
        funcstr += "{l}, ".format(l=letters[j])

    funcstr += "{l}>(int start, int end, ".format(l=letters[i])

    for j in range(i + 1):
        adjusted = j + 1
        funcstr += "Buffer<{l}> buf{a}, ".format(l=letters[j], a=adjusted)

    funcstr += "Action<Index, "

    for j in range(i):
        adjusted = j + 1
        funcstr += "GPUArray<{l}>, ".format(l=letters[j])

    funcstr += "GPUArray<{l}>> action)".format(l=letters[i])

    for j in range(i + 1):
        funcstr += "\n    where {l} : unmanaged".format(l=letters[j])

    funcstr += """
{
    var handler = new AcceleratorHandler();
    handler.DispatchKernel(start, end, """

    for j in range(i + 1):
        adjusted = j + 1
        funcstr += "buf{a}, ".format(a=adjusted)

    funcstr += """action);
}

"""

    ofile.write(funcstr)
