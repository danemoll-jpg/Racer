# Standing ramp validation requirement (CR-077)

Every future new or modified ramp must be tested at its **current authored position**, using ordinary-frame motor/input/physics traversal. A nominal pilot, route bypass, old fixture, or teleport through the ramp is insufficient.

Record course and ramp identity, source/build identity, eligible vehicle, target and measured approach speed, starting line, takeoff, landing, contact/suspension anomalies, recovery and result. Retain failures. Distinguish an expected extreme off-line crash from routine centered/off-center snagging.

Required matrix:

- Every vehicle eligible for that course.
- Intended low, normal and high approach speeds; state those values explicitly.
- Centered and moderately off-center approaches, plus both side-edge contacts.
- Actual takeoff and landing, with upright/support checks after landing.
- Local recovery beside and beneath the ramp and at the landing.
- AI approach, completion, sustained-stall recovery and subsequent progress where applicable.
- Forward/reverse and adjacent-ramp regression runs where shared geometry is involved.

`ArcadeRampProbe` and `Tools/Validate-CR075-080.ps1 -Group ramps` exercise the actual Trickum frame with 12/24/36 m/s targets, supplemented by the 43 m/s (155 km/h) high-speed matrix, three interior lines and both side contacts. The initial placement is outside the measured approach. Actual frame traces and failed outcomes are written per case. This is a diagnostic framework, not a blanket statement that an arbitrary future ramp has passed. Update the target speeds and route-specific pilot when the authored ramp or intended speed range changes.

Also run multi-lap races and check penalties, legal bypasses, recovery loops, wrong-way feedback and performance. Keep rules fixtures, automated physical pilots and Dan's hands-on/controller review separate.
