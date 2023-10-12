lines = []

with open("README.md", "r") as file:
    lines = file.readlines()

begin_idx = next(i for i, line in enumerate(lines) if "## Contributors" in line)
end_idx = next(
    i for i, line in enumerate(lines) if "<!-- ALL-CONTRIBUTORS-LIST:END -->" in line
)

del lines[begin_idx : end_idx + 1]

with open("ProcessedREADME.md", "w") as file:
    file.writelines(lines)
