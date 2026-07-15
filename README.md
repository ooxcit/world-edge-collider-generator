# Oox World Edge Collider Generator

Generates a chain of box colliders along a path of points placed and edited directly in the Scene view.

## Features
- Place points by clicking in the Scene view.
- Drag any existing point with a handle, or edit the list numerically in the inspector.
- Generate box colliders along the path, closing the loop from the last point back to the first.
- Clear the generated colliders (or the points) and start over.

## Installation
1. Add this package to your Unity project via the Package Manager or by including it as a Git dependency in your `manifest.json` file:
   ```json
   {
      "dependencies": {
        "com.oox.world-edge-collider-generator": "https://github.com/ooxcit/world-edge-collider-generator.git"
      }
   }
   ```
2. Open Unity, and the package will automatically be installed.

## Usage
1. Add the **World Edge Collider Generator** component to a `GameObject` in your scene.
2. Assign a **Box Parent** transform — the generated `BoxCollider` `GameObject`s will be parented under it.
3. Set the **Height** and **Thickness** of the generated colliders.
4. Select the component and press **Place Points** in the inspector, then click in the Scene view to add points along the edge you want to cover. Press the button again to stop placing points.
5. Adjust any point by dragging its handle in the Scene view, or by editing the **Points** list in the inspector.
6. Press **Generate Box Colliders** to create the colliders. Press **Clear Box Colliders** to remove them, or **Clear Points** to reset the path.