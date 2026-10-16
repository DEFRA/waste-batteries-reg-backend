"""Fail when WasteBatteriesRegBackend line coverage is below the given percent.

Counts lines in the production assembly. Generated files under obj are excluded.
Types marked ExcludeFromCodeCoverage are already absent from the report.
"""

import sys
import xml.etree.ElementTree as ET


def local_name(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]


def children(element: ET.Element, name: str) -> list[ET.Element]:
    return [child for child in element if local_name(child.tag) == name]


def line_coverage(path: str) -> tuple[int, int]:
    root = ET.parse(path).getroot()
    covered = 0
    total = 0
    package_lists = children(root, "packages")
    if not package_lists:
        return covered, total

    for package in children(package_lists[0], "package"):
        if package.get("name") != "WasteBatteriesRegBackend":
            continue

        for classes in children(package, "classes"):
            for cls in children(classes, "class"):
                filename = (cls.get("filename") or "").replace("\\", "/")
                if "/obj/" in f"/{filename}":
                    continue

                for lines in children(cls, "lines"):
                    for line in children(lines, "line"):
                        total += 1
                        if int(line.get("hits") or "0") > 0:
                            covered += 1

    return covered, total


def main() -> int:
    if len(sys.argv) != 3:
        print("Usage: assert-line-coverage.py <cobertura.xml> <threshold-percent>", file=sys.stderr)
        return 2

    path = sys.argv[1]
    threshold = float(sys.argv[2])
    covered, total = line_coverage(path)

    if total == 0:
        print("::error::No instrumented WasteBatteriesRegBackend lines were found in the coverage report.")
        return 1

    percent = 100 * covered / total
    print(f"Line coverage is {percent:.2f}% ({covered}/{total}). Threshold is {threshold:g}%.")

    if covered * 100 < threshold * total:
        print(f"::error::Line coverage {percent:.2f}% is below threshold {threshold:g}%.")
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
