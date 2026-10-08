# Mech Battle project

## Summary
Tech demo about controlling titan-class battle mech on battlefield contains thousand of small enemies and destructible environment.

## Technical key features
- Custom navigation system based on triaxial coords system. All map divided into hexagons, and navigation path construction also have 2 layers: constructing path through hex-edge portals and constructing triangle path inside hexagon (also have 2 options - calculated path and pre-calculated flow map leading to selected hex portal). All navigation agents utilizes the same grid, despite their radiuses. Collision avoidance system also utilizes same grid.
- ECS approach, provided by Morpeh ECS. Triangle grid is realized as cell = entity, which gives ability to add any required data directly to cell (via components)
- Use of Unity.Jobs directly in Morpeh systems (provided by NativeFilters \ NativeStashes) and indirectly by divide loading in multiple separate processes. Most of transform calculations (ex. hierarchy update), applications (TransformArray) and raycasts (Raycast.Schedule) are done inside jobs. All mono-objects are view-only, all data is controlled by ECS.
- Target small units count is 10.000 units
- IK calculation for mech movement

## Game key features
- Player controls a giant mech with multiple weapon slots (2 primary weapons, laser eyes, additional corpus slots(not realized)). Configurable slots setup.
- Unique mech health system - mech have multiple energy cells in every partition. Most charged partition receives damage and spend energy to dissipate it. When energy depleted, cells are taking health damage. When all partition cells are broken, damage are applied to mech's heart - the reactor. If reactor hp is depleted, mech is destroyed. Damaged partition can be disabled (not implemented) - disabled arm prevent its slots to work, disabled leg interrupts regular movement.
- Spending energy for shots and for consuming incoming damage raises adrenaline level. Higher mech adrenaline means more energy production, hovewer less repair speed.
- Mech steps trample units and objects below it
- Units are controlled in squads - they can move, guard and attack both mechs and other squads. Indirect squads control planned (follow, stay, capture, attack)

## Planned features
### Mech
- shield projecting : key feature. Mech should produce spherical shield around it, protecting itself and nearby units. Will be used in missions.
- support modules on arms, activated with numerical keys (as abilities). These can be guns, shield boosters, anti-aircraft turrets, etc.
- destroyed mech hull view and its physics
- mech model rework
- aoe damage for primary and secondary guns

### Units
- destroyed units hull views
- capturing strategy points with squads
- additional unit types: artillery, small mechs
- aviation (also will use triangle grid height data for navigation) and counter-aviation units
- flying and underground squad reinforcement transporters
- unit-buildings - bunkers, artillery towers, shield complexes

### Environment
- destructible environment and debris terrain layer - units hulls and terrain objects splits in simple forms (cubes/spheres) on destruction and bakes into terrain. It changes terrain cells texture and height (also using triangle grid). Use of destructible maps for complex objects, where every voxel encoding durability & material
- big volume sdf vfx effects - fire, fog, smoke.
- active level zone restrictions
- background terrains visual (out of active level zone)

### Interface
- minimap displaying captured hexes with color
- tactical mode, activated by ALT button - shows squad symbols and other mechs health, highlights strategic points and fronline
- understandable mech chassis controls elements, better controlled steer and speed