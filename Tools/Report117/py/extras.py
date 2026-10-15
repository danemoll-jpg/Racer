# 0.117: the kickers on Leviathan Reverse's shortcuts (Part C items 9 and 10) and the scene signs relabelled for Leviathan (Part B item 4).
# Rocky Way Cut: its line climbs the open slope and drops ~5 m off the bank onto the lake path's west side: a kicker at the bank's edge, so it
# is a jump off the bank (landing on the bank's down-slope and the path), not a drop. Hollow Creek Hop: its line crosses a 1.8 m creek channel:
# a kicker on the near side, so it hops the channel. Both on the existing ground, race only (LeviathanCourse.BuildKickers).
# usage: python Tools/Report117/py/extras.py
import json, math
def pts(b): return [tuple(b['pts'][i:i + 3]) for i in range(0, len(b['pts']), 3)]
def stations(P):
    S = [0.0]
    for i in range(1, len(P)): S.append(S[-1] + math.dist(P[i], P[i - 1]))
    return S
def heading(a, b): return math.degrees(math.atan2(b[0] - a[0], b[2] - a[2])) % 360
R = json.load(open('Assets/Resources/Leviathan/LeviathanReverse.json'))
kick = []
b = next(x for x in R['branches'] if x['title'] == 'Rocky Way Cut'); P = pts(b); S = stations(P)
# the bank's edge: the last point before the line drops more than 2.5 m over the next 8 m
i = next(k for k in range(len(P) - 5) if P[k][1] - min(P[j][1] for j in range(k, len(P)) if S[j] - S[k] <= 8) > 2.5)
kick.append({'at': 'Rocky Way Cut, the bank', 'pos': [round(P[i][0], 2), round(P[i][2], 2), round(P[i][1], 2)], 'yaw': round(heading(P[i - 3], P[i]), 1), 'width': 7.0, 'length': 6.0, 'height': .75})
b = next(x for x in R['branches'] if x['title'] == 'Hollow Creek Hop'); P = pts(b); S = stations(P)
lo = min(range(len(P)), key=lambda k: P[k][1])  # the channel's bottom
i = max(k for k in range(lo) if P[lo][1] < P[k][1] - 1.2 and S[lo] - S[k] >= 5)  # the near bank, ~1.2 m above the bottom
kick.append({'at': 'Hollow Creek Hop, the creek', 'pos': [round(P[i][0], 2), round(P[i][2], 2), round(P[i][1], 2)], 'yaw': round(heading(P[i - 3], P[i]), 1), 'width': 8.0, 'length': 6.0, 'height': .8})
for k in kick: print(k)
R['kickers'] = kick
# Rocky Way Cut with the driveway entrance (0.117): the rivals drive it in ~17 s against 13.5-14.5 s by the main road and stall rejoining
# after its jump (AI-only races: 4 of 9 DNF); the pad drove it in 8.2 s. It stays a shortcut for the player; the rivals do not choose it.
for b in R['branches']:
    if b['title'] == 'Rocky Way Cut': b['aiValidated'] = False
R['relabel'] = [{'find': 'FOLLOW GOLD', 'text': 'LEVIATHAN\nDOWNHILL RIDGE CUT'}, {'find': 'SHORTCUT 2', 'text': '<<< LEVIATHAN\nDOWNHILL RIDGE CUT'},
                {'find': 'SUMMIT TRAVERSE', 'text': 'SUMMIT TRAVERSE\nROAD CLOSED'}]
open('Assets/Resources/Leviathan/LeviathanReverse.json', 'w', encoding='utf-8', newline='\n').write(json.dumps(R, separators=(',', ':')))
F = json.load(open('Assets/Resources/Leviathan/LeviathanForward.json'))
F['relabel'] = [{'find': 'CLIMBING RIDGE CUT', 'text': 'LEVIATHAN\nCLIMBING RIDGE CUT'}, {'find': 'SUMMIT TRAVERSE', 'text': 'SUMMIT TRAVERSE\nROAD CLOSED'}]
open('Assets/Resources/Leviathan/LeviathanForward.json', 'w', encoding='utf-8', newline='\n').write(json.dumps(F, separators=(',', ':')))
