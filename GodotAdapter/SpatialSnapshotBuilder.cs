using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using XPrimeRay.Core.Geometry;
using XPrimeRay.Spatial;
using GdVector3 = Godot.Vector3;
using NumericsVector3 = System.Numerics.Vector3;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace GodotAdapter;

/// <summary>
/// Narrow Godot-to-spatial-kernel adapter. It deliberately accepts only
/// enabled BoxShape3D primitives for formal dual validation.
/// </summary>
public static class SpatialSnapshotBuilder
{
    public const string SchemaVersion = FrozenGeometrySnapshot.SchemaVersion;

    public static FrozenGeometrySnapshot BuildFromGodotScene(Node root)
    {
        Node sceneRoot = root?.GetTree()?.CurrentScene ?? root
            ?? throw new InvalidOperationException("A scene root is required.");
        List<ShapeRecord> records = new();
        CollectShapes(sceneRoot, records);

        var ordered = records
            .GroupBy(r => r.OwnerPath, StringComparer.Ordinal)
            .SelectMany(group => group.OrderBy(r => r.ShapePath, StringComparer.Ordinal)
                .Select((record, ordinal) => record with { ShapeOrdinal = ordinal }))
            .OrderBy(r => r.ShapePath, StringComparer.Ordinal)
            .ToArray();

        List<FrozenOrientedBox> primitives = new(ordered.Length);
        foreach (ShapeRecord record in ordered)
        {
            if (record.Shape.Shape is not BoxShape3D box)
                throw new NotSupportedException($"Spatial Kernel v1 supports only BoxShape3D; unsupported={record.Shape.GetPath()}");

            Transform3D godotTransform = record.Shape.GlobalTransform;
            Matrix4x4 worldFromLocal = ToNumerics(godotTransform);
            if (!Matrix4x4.Invert(worldFromLocal, out Matrix4x4 localFromWorld))
                throw new InvalidOperationException($"Non-invertible geometry transform={record.Shape.GetPath()}");

            NumericsVector3 halfExtents = ToNumerics(box.Size) * 0.5f;
            Aabb3 worldBounds = BuildWorldBounds(worldFromLocal, halfExtents);
            string canonicalId = $"{record.ShapePath}#{record.ShapeOrdinal}";
            FrozenPrimitiveFlags flags = record.Excluded
                ? FrozenPrimitiveFlags.Excluded
                : FrozenPrimitiveFlags.CollideWithBodies;
            primitives.Add(new FrozenOrientedBox(
                canonicalId,
                SpatialSurfaceClass.Geometry,
                worldFromLocal,
                localFromWorld,
                halfExtents,
                worldBounds,
                record.Owner.CollisionLayer,
                flags));
        }

        return new FrozenGeometrySnapshot(primitives);
    }

    private static void CollectShapes(Node node, List<ShapeRecord> records)
    {
        if (node is CollisionShape3D shape && shape.Shape != null)
        {
            CollisionObject3D? owner = FindCollisionOwner(shape);
            if (owner != null && owner.CollisionLayer != 0)
            {
                string ownerPath = owner.GetPath().ToString();
				if (owner.IsInGroup("spatial_kernel_excluded"))
					return;
                records.Add(new ShapeRecord(
                    shape,
                    owner,
                    ownerPath,
                    shape.GetPath().ToString(),
                    false,
                    0));
            }
        }

        foreach (Node child in node.GetChildren())
            CollectShapes(child, records);
    }

    private static CollisionObject3D? FindCollisionOwner(Node node)
    {
        for (Node? current = node.GetParent(); current != null; current = current.GetParent())
            if (current is CollisionObject3D owner) return owner;
        return null;
    }

    private static Matrix4x4 ToNumerics(Transform3D t)
    {
        GdVector3 bx = t.Basis.X;
        GdVector3 by = t.Basis.Y;
        GdVector3 bz = t.Basis.Z;
        GdVector3 o = t.Origin;
        return new Matrix4x4(
            bx.X, by.X, bz.X, 0f,
            bx.Y, by.Y, bz.Y, 0f,
            bx.Z, by.Z, bz.Z, 0f,
            o.X, o.Y, o.Z, 1f);
    }

    private static NumericsVector3 ToNumerics(GdVector3 v) => new(v.X, v.Y, v.Z);

    private static Aabb3 BuildWorldBounds(Matrix4x4 worldFromLocal, NumericsVector3 halfExtents)
    {
        NumericsVector3 min = new(float.PositiveInfinity);
        NumericsVector3 max = new(float.NegativeInfinity);
        for (int i = 0; i < 8; i++)
        {
            NumericsVector3 local = new(
                (i & 1) != 0 ? halfExtents.X : -halfExtents.X,
                (i & 2) != 0 ? halfExtents.Y : -halfExtents.Y,
                (i & 4) != 0 ? halfExtents.Z : -halfExtents.Z);
            NumericsVector3 world = NumericsVector3.Transform(local, worldFromLocal);
            min = NumericsVector3.Min(min, world);
            max = NumericsVector3.Max(max, world);
        }
        return new Aabb3(min, max);
    }

    private readonly record struct ShapeRecord(
        CollisionShape3D Shape,
        CollisionObject3D Owner,
        string OwnerPath,
        string ShapePath,
        bool Excluded,
        int ShapeOrdinal);
}
