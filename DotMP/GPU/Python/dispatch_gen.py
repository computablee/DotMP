ofile = open("./dispatch_dump.cs", "w")

cardinals = ["one", "two", "three", "four", "five", "six", "seven", "eight",
             "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen"]
ordinals = ["first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth",
            "ninth", "tenth", "eleventh", "twelfth", "thirteenth", "fourteenth", "fifteenth", "sixteenth"]

letters = ["T", "U", "V", "W", "X", "Y", "Z",
           "A", "B", "C", "D", "E", "F", "G", "H", "I"]

for i in range(0, 13):
    funcstr = ""

    funcstr += """/// <summary>
/// Dispatches a kernel with {c} parameters.
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
internal void DispatchKernel<"""

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
    var idx = new Index();

    var kernel = accelerator.LoadStreamKernel(action);

    kernel(((end - start) / block_size, block_size), idx,
"""

    for j in range(i):
        adjusted = j + 1
        funcstr += """        new GPUArray<{l}>(buf{a}.View),
""".format(l=letters[j], a=adjusted)

    funcstr += """        new GPUArray<{l}>(buf{a}.View));

    Synchronize();
""".format(l=letters[i], a=i + 1)

    funcstr += "}\n\n"

    ofile.write(funcstr)
