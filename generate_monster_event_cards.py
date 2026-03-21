from __future__ import annotations

import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parent
TEMP_LOC = ROOT / "temp" / "Slay the Spire 2" / "localization"
MONSTER_ZHS = TEMP_LOC / "zhs" / "monsters.json"
MONSTER_ENG = TEMP_LOC / "eng" / "monsters.json"
CARDS_DIR = ROOT / "Cards" / "MonsterSouls"
REGISTRY_FILE = CARDS_DIR / "MonsterSoulCardRegistry.cs"
ZHS_CARDS = ROOT / "ABStS2Mod" / "localization" / "zhs" / "cards.json"
ENG_CARDS = ROOT / "ABStS2Mod" / "localization" / "eng" / "cards.json"
NAMESPACE = "ABStS2Mod.Cards.MonsterSouls"


def to_pascal(raw: str) -> str:
    parts = re.split(r"[^A-Za-z0-9]+", raw.strip())
    built = []
    for part in parts:
        if not part:
            continue
        if part[0].isdigit():
            built.append("N" + part)
        else:
            built.append(part[0].upper() + part[1:].lower())
    return "".join(built)


def to_upper_snake(pascal: str) -> str:
    snake = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", pascal)
    snake = re.sub(r"([A-Z]+)([A-Z][a-z])", r"\1_\2", snake)
    return snake.upper()


def load_json(path: Path) -> dict[str, str]:
    return json.loads(path.read_text(encoding="utf-8"))


def write_json(path: Path, data: dict[str, str]) -> None:
    text = json.dumps(data, ensure_ascii=False, indent=2) + "\n"
    path.write_text(text, encoding="utf-8")


def card_source(class_name: str) -> str:
    return f"""using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace {NAMESPACE};

[Pool(typeof(ColorlessCardPool))]
public sealed class {class_name}() : CustomCardModel(0, CardType.Skill, CardRarity.Event, TargetType.Self)
{{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] {{ new CardsVar(1) }};

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {{
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }}
}}
"""

def registry_source(entries: list[tuple[str, str]]) -> str:
    lines = []
    for monster_id, class_name in entries:
        lines.append(f'            "{monster_id}" => owner.RunState.CreateCard<{class_name}>(owner),')
    body = "\n".join(lines)
    return f"""using ABStS2Mod.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Cards.MonsterSouls;

public static class MonsterSoulCardRegistry
{{
    public static CardModel Create(Player owner, string monsterId)
    {{
        string normalizedId = monsterId.StartsWith("MONSTER.") ? monsterId["MONSTER.".Length..] : monsterId;
        return normalizedId switch
        {{
{body}
            _ => owner.RunState.CreateCard<SoulCaptureTest>(owner),
        }};
    }}
}}
"""


def main() -> None:
    monsters_zhs = load_json(MONSTER_ZHS)
    monsters_eng = load_json(MONSTER_ENG)
    name_entries = [k for k in monsters_zhs.keys() if k.endswith(".name")]
    ids = sorted({k[:-5] for k in name_entries})

    CARDS_DIR.mkdir(parents=True, exist_ok=True)

    zhs_cards = load_json(ZHS_CARDS)
    eng_cards = load_json(ENG_CARDS)
    generated: list[tuple[str, str, str]] = []
    seen_class: dict[str, int] = {}

    for monster_id in ids:
        base_class = "SoulMonster" + to_pascal(monster_id)
        count = seen_class.get(base_class, 0)
        seen_class[base_class] = count + 1
        class_name = f"{base_class}{count + 1}" if count > 0 else base_class
        file_path = CARDS_DIR / f"{class_name}.cs"
        file_path.write_text(card_source(class_name), encoding="utf-8")
        key = f"ABSTS2MOD-{to_upper_snake(class_name)}"
        zhs_name = monsters_zhs.get(f"{monster_id}.name", monster_id)
        eng_name = monsters_eng.get(f"{monster_id}.name", monster_id)
        zhs_cards[f"{key}.title"] = zhs_name
        zhs_cards[f"{key}.description"] = "抽 [blue]1[/blue] 张牌。"
        eng_cards[f"{key}.title"] = eng_name
        eng_cards[f"{key}.description"] = "Draw [blue]1[/blue] card."
        generated.append((monster_id, class_name, key))

    registry_entries = [(monster_id, class_name) for monster_id, class_name, _ in generated]
    REGISTRY_FILE.write_text(registry_source(registry_entries), encoding="utf-8")

    write_json(ZHS_CARDS, zhs_cards)
    write_json(ENG_CARDS, eng_cards)
    print(f"generated_cards={len(generated)}")
    print(f"output_dir={CARDS_DIR}")


if __name__ == "__main__":
    main()
