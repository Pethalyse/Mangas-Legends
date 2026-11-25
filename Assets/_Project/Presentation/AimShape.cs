using MangaLegends.Data.Definitions;
using MangaLegends.Data.Effects;
using MangaLegends.Simulation.Abilities;

namespace MangaLegends.Presentation
{
    public enum AimShapeKind
    {
        None,
        Line,
        Cone,
        Circle
    }

    // Reads the indicator shape from what the cast spawns, so that champion data needs no extra indicator fields;
    // TargetingData's indicator width and radius still override it when set.
    public struct AimShape
    {
        private const float PointMarkerRadius = 0.6f;

        public float Range;
        public AimShapeKind Kind;
        public float Length;
        public float Width;
        public float Radius;
        public float Angle;
        public int Count;
        public float Spread;
        public bool AtCursor;

        public static AimShape Describe(in AimSource source, float casterRadius)
        {
            TargetingData targeting = source.Targeting;
            int rank = source.Rank;
            AimShape shape = new AimShape { Range = targeting.RangeAt(rank), Count = 1 };
            bool found = false;
            float dashLength = 0f;
            if (source.Effects != null)
                foreach (EffectDefinition effect in source.Effects)
                {
                    if (found) break;
                    switch (effect)
                    {
                        case LaunchProjectileEffect launch when launch.Projectile != null:
                            shape.Kind = AimShapeKind.Line;
                            shape.Length = launch.Projectile.MaxDistanceAt(rank);
                            shape.Width = launch.Projectile.RadiusAt(rank) * 2f;
                            shape.Count = launch.CountAt(rank);
                            shape.Spread = launch.SpreadAngleAt(rank);
                            found = true;
                            break;
                        case CreateAreaEffect create when create.Area != null:
                            Describe(create, rank, ref shape);
                            found = true;
                            break;
                        case DashEffect dash:
                            dashLength = dash.MaxDistanceAt(rank);
                            break;
                    }
                }

            if (!found && dashLength > 0f)
            {
                shape.Kind = AimShapeKind.Line;
                shape.Length = dashLength;
                shape.Width = casterRadius * 2f;
            }
            if (targeting.Mode == TargetingMode.Unit) shape.Kind = AimShapeKind.None;
            if (targeting.Mode == TargetingMode.Point && (shape.Kind == AimShapeKind.None || shape.Kind == AimShapeKind.Line))
            {
                shape.Kind = AimShapeKind.Circle;
                shape.AtCursor = true;
                shape.Radius = PointMarkerRadius;
            }

            float width = targeting.IndicatorWidthAt(rank);
            float radius = targeting.IndicatorRadiusAt(rank);
            if (width > 0f) shape.Width = width;
            if (radius > 0f) shape.Radius = radius;
            if (shape.Range <= 0f && shape.Kind == AimShapeKind.Line) shape.Range = shape.Length;
            return shape;
        }

        private static void Describe(CreateAreaEffect create, int rank, ref AimShape shape)
        {
            AreaDefinition area = create.Area;
            switch (area.Shape)
            {
                case AreaShape.Cone:
                    shape.Kind = AimShapeKind.Cone;
                    shape.Radius = area.RadiusAt(rank);
                    shape.Angle = area.AngleAt(rank);
                    break;
                case AreaShape.Rectangle:
                    shape.Kind = AimShapeKind.Line;
                    shape.Length = area.LengthAt(rank);
                    shape.Width = area.WidthAt(rank);
                    break;
                default:
                    shape.Kind = AimShapeKind.Circle;
                    shape.Radius = area.RadiusAt(rank);
                    shape.AtCursor = create.AtAimPoint;
                    break;
            }
        }
    }
}
