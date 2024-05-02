namespace TheScriptingEngineer
{
    using System;
    using System.IO; // for path operations
    using System.Collections.Generic; // for using list
    using NXOpen;
    using NXOpen.CAE; // so we don't need to start everything with NXOpen.CAE
    using NXOpen.Utilities;
    using NXOpen.UF;
    using NXOpenUI;

    public class AssemblyMoveComponent
    {
        static NXOpen.Session theSession = NXOpen.Session.GetSession();
        static ListingWindow theLW = theSession.ListingWindow;
        static BasePart basePart = theSession.Parts.BaseWork;
        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);
            theLW.WriteFullline("Funcitons RotateComponentY and RotateComponentX are UNCHECKED use at own risk");

            NXOpen.Part workPart = (NXOpen.Part)basePart;
            NXOpen.Assemblies.Component component1 = ((NXOpen.Assemblies.Component)workPart.ComponentAssembly.RootComponent.FindObject("COMPONENT model1 1"));

            //TranslateComponent(workPart, component1, 0, -100, 0);
            RotateComponentZ(workPart, component1, -30);

        }

        /// <summary>
        /// Translates a component in a part.
        /// </summary>
        /// <param name="workPart">The part in which to move the component.</param>
        /// <param name="componentToTranslate">The component to translate.</param>
        /// <param name="dX">Distance to translate in X-direction.</param>
        /// <param name="dY">Distance to translate in Y-direction.</param>
        /// <param name="dZ">Distance to translate in Z-direction.</param>
        public static void TranslateComponent(Part workPart, NXOpen.Assemblies.Component componentToTranslate, double dX, double dY, double dZ)
        {
            // now that we have the component, so we can move it
            Vector3d translate;
            translate.X = dX;
            translate.Y = dY;
            translate.Z = dZ;
            Matrix3x3 rotate;
            rotate.Xx = 1;
            rotate.Xy = 0;
            rotate.Xz = 0;
            rotate.Yx = 0;
            rotate.Yy = 1;
            rotate.Yz = 0;
            rotate.Zx = 0;
            rotate.Zy = 0;
            rotate.Zz = 1;
            workPart.ComponentAssembly.MoveComponent(componentToTranslate, translate, rotate);
        }


        /// <summary>
        /// Rotates a component in an assembly around the global z-axis.
        /// </summary>
        /// <param name="workPart">The part in which to move the component.</param>
        /// <param name="componentToRotate">The component to translate.</param>
        /// <param name="angle">Angle to rotate around z-axis in degrees.</param>
        public static void RotateComponentZ(Part workPart, NXOpen.Assemblies.Component componentToRotate, double angle)
        {
            // now that we have the component, so we can move it
            Vector3d translate;
            translate.X = 0;
            translate.Y = 0;
            translate.Z = 0;
            Matrix3x3 rotate;
            rotate.Xx = System.Math.Cos(angle / 180 * System.Math.PI);
            rotate.Xy = -System.Math.Sin(angle / 180 * System.Math.PI);
            rotate.Xz = 0;
            rotate.Yx = System.Math.Sin(angle / 180 * System.Math.PI);
            rotate.Yy = System.Math.Cos(angle / 180 * System.Math.PI);
            rotate.Yz = 0;
            rotate.Zx = 0;
            rotate.Zy = 0;
            rotate.Zz = 1;
            workPart.ComponentAssembly.MoveComponent(componentToRotate, translate, rotate);
        }


        /// <summary>
        /// Rotates a component in an assembly around the global y-axis.
        /// </summary>
        /// <param name="workPart">The part in which to move the component.</param>
        /// <param name="componentToRotate">The component to translate.</param>
        /// <param name="angle">Angle to rotate around y-axis in degrees.</param>
        public static void RotateComponentY(Part workPart, NXOpen.Assemblies.Component componentToRotate, double angle)
        {
            // now that we have the component, so we can move it
            Vector3d translate;
            translate.X = 0;
            translate.Y = 0;
            translate.Z = 0;
            Matrix3x3 rotate;
            rotate.Xx = System.Math.Cos(angle / 180 * System.Math.PI);
            rotate.Xy = 0;
            rotate.Xz = -System.Math.Sin(angle / 180 * System.Math.PI);
            rotate.Yx = 0;
            rotate.Yy = 1;
            rotate.Yz = 0;
            rotate.Zx = System.Math.Sin(angle / 180 * System.Math.PI);
            rotate.Zy = 0;
            rotate.Zz = System.Math.Cos(angle / 180 * System.Math.PI);
            workPart.ComponentAssembly.MoveComponent(componentToRotate, translate, rotate);
        }


        /// <summary>
        /// Rotates a component in an assembly around the global x-axis.
        /// </summary>
        /// <param name="workPart">The part in which to move the component.</param>
        /// <param name="componentToRotate">The component to translate.</param>
        /// <param name="angle">Angle to rotate around x-axis in degrees.</param>
        public static void RotateComponentX(Part workPart, NXOpen.Assemblies.Component componentToRotate, double angle)
        {
            // now that we have the component, so we can move it
            Vector3d translate;
            translate.X = 0;
            translate.Y = 0;
            translate.Z = 0;
            Matrix3x3 rotate;
            rotate.Xx = 1;
            rotate.Xy = 0;
            rotate.Xz = 0;
            rotate.Yx = 0;
            rotate.Yy = System.Math.Cos(angle / 180 * System.Math.PI);
            rotate.Yz = -System.Math.Sin(angle / 180 * System.Math.PI);
            rotate.Zx = 0;
            rotate.Zy = System.Math.Sin(angle / 180 * System.Math.PI);
            rotate.Zz = System.Math.Cos(angle / 180 * System.Math.PI);
            workPart.ComponentAssembly.MoveComponent(componentToRotate, translate, rotate);
        }
    }
}
