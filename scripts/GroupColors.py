import requests
import re
import json
import time

BASE_URL = "https://alexbeals.com/projects/colorize/search.php"

GROUP_NAMES = [
    "Alcohol",
    "Aldehydic",
    "Almond",
    "Amber",
    "Animalic",
    "Anis",
    "Aquatic",
    "Aromatic",
    "Asphault",
    "Balsamic",
    "Beeswax",
    "Bitter",
    "Brown Scotch Tape",
    "Cacao",
    "Camphor",
    "Cannabis",
    "Caramel",
    "Champagne",
    "Cherry",
    "Chocolate",
    "Cinnamon",
    "Citrus",
    "Clay",
    "Coca Cola",
    "Coconut",
    "Coffee",
    "Conifer",
    "Creamy",
    "Earthy",
    "Floral",
    "Fresh",
    "Fresh Spicy",
    "Fruity",
    "Gasoline",
    "Gourmand",
    "Green",
    "Herbal",
    "Honey",
    "Hot Iron",
    "Industrial Glue",
    "Iris",
    "Lactonic",
    "Lavender",
    "Leather",
    "Marine",
    "Metallic",
    "Mineral",
    "Mossy",
    "Musky",
    "Nutty",
    "Oily",
    "Oriental",
    "Oud",
    "Ozonic",
    "Paper",
    "Patchouli",
    "Plastic",
    "Powdery",
    "Rose",
    "Rubber",
    "Rum",
    "Salty",
    "Sand",
    "Savory",
    "Smoky",
    "Soapy",
    "Soft Spicy",
    "Sour",
    "Spicy",
    "Sweet",
    "Terpenic",
    "Tobacco",
    "Tropical",
    "Tuberose",
    "Vanilla",
    "Vinyl",
    "Violet",
    "Vodka",
    "Warm Spicy",
    "Whiskey",
    "White Floral",
    "Wine",
    "Woody",
    "Yellow Floral",
]

COLOR_REGEX = re.compile(
    r"background-color:\s*(#[0-9A-Fa-f]{6})",
    re.IGNORECASE
)

def fetch_color_for_word(word: str) -> str | None:
    resp = requests.get(BASE_URL, params={"q": word}, timeout=10)
    resp.raise_for_status()

    match = COLOR_REGEX.search(resp.text)
    if not match:
        print(f"[WARN] No background-color found for '{word}'")
        return None

    color = match.group(1).upper()
    print(f"[OK] {word} -> {color}")
    return color


def main():
    mapping: dict[str, str | None] = {}

    for name in GROUP_NAMES:
        color = fetch_color_for_word(name)
        mapping[name] = color
        time.sleep(0.3)

    print("\n=== JSON mapping ===")
    print(json.dumps(mapping, indent=2))

    print("\n=== TypeScript mapping ===")
    print("export const GROUP_COLORS: Record<string, string | null> = {")
    for name, color in mapping.items():
        safe_name = name.replace('"', '\\"')
        color_literal = f'"{color}"' if color is not None else "null"
        print(f'  "{safe_name}": {color_literal},')
    print("};")

if __name__ == "__main__":
    main()
