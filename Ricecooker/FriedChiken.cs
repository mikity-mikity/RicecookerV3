using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mikity;
using mikity.LinearAlgebra;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using mikity.NumericalMethodHelper;
using mikity.NumericalMethodHelper.elements;
namespace mikity.ghComponents
{

    public class GH_FriedChikenMainLoop : Grasshopper.Kernel.GH_Component
    {
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.icon29.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
        

        public GH_FriedChikenMainLoop()
            : base("MainLoop", "MainLoop", "MainLoop", "Ricecooker", "Computation")
        {
        }
        ~GH_FriedChikenMainLoop()
        {
            keyboardHook.Uninstall();
        }
        RamGecTools.MouseHook mouseHook = new RamGecTools.MouseHook();
        RamGecTools.KeyboardHook keyboardHook = new RamGecTools.KeyboardHook();
        void keyboardHook_KeyUp(RamGecTools.KeyboardHook.VKeys key)
        {
            if (key == RamGecTools.KeyboardHook.VKeys.ESCAPE)
            {
                _go = false;
                t = 0;
                timer.Enabled = false;
                ExpireSolution(true);
            }
            if (key == RamGecTools.KeyboardHook.VKeys.SPACE)
            {
                if (_go)
                {
                    _go = false;
//                    t = 0;
                    timer.Enabled = false;
                }
                else
                {
                    _go = true;
                    t = 0;
                    timer.Enabled = true;
                }
            }
        }

        void keyboardHook_KeyDown(RamGecTools.KeyboardHook.VKeys key)
        {

            //keyboardKeyPress.BackColor = Color.IndianRed;
            //keyboardLog.Text = "[" + DateTime.Now.ToLongTimeString() + "] KeyDown Event {" + key.ToString() + "}" + Environment.NewLine + keyboardLog.Text;
        }
        void timer_Tick(object sender, EventArgs e)
        {
            this.ExpireSolution(true);
        }
        System.Windows.Forms.Timer timer;
        public override void AddedToDocument(Grasshopper.Kernel.GH_Document document)
        {
            base.AddedToDocument(document);
            //Rhino.RhinoDoc.ReplaceRhinoObject += RhinoDoc_ReplaceRhinoObject;
            timer = new System.Windows.Forms.Timer();
            timer.Tick += timer_Tick;
            timer.Enabled = false;
            timer.Interval = 1;

            // register evens
            keyboardHook.KeyDown += new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyDown);
            keyboardHook.KeyUp += new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyUp);

            keyboardHook.Install();


        }

        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ParticleSystems", "pS", "ParticleSystems", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddNumberParameter("Mass", "Mass", "Mass of one particle", Grasshopper.Kernel.GH_ParamAccess.item,1.0);
            pManager.AddNumberParameter("Speed", "dt", "Big dt may cause trouble!", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "D", "Distance from the original model", Grasshopper.Kernel.GH_ParamAccess.item, 10.0);
        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "Geom", "Geometry", Grasshopper.Kernel.GH_ParamAccess.list);

            pManager.AddTextParameter("Elapsed", "dbg", "Elapsed Time", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        public override void BakeGeometry(Rhino.RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, List<Guid> obj_ids)
        {
            if (lpS != null)
            {
                foreach (GH_particleSystem pS in lpS)
                {
                    if (pS != null)
                    {

                        if (pS.BKGT != null)
                        {
                                pS.BKGT(doc,att,obj_ids);
                        }
                    }
                }
            }
            base.BakeGeometry(doc, att, obj_ids);
        }
        public override void DrawViewportWires(Grasshopper.Kernel.IGH_PreviewArgs args)
        {
            if (lpS != null)
            {
                foreach (GH_particleSystem pS in lpS)
                {
                    if (pS != null)
                    {

                        if (pS.DVPW != null)
                        {
                            pS.DVPW(args);
                        }
                    }
                }
            }
            base.DrawViewportWires(args);
        }

        System.Windows.Forms.ToolStripMenuItem __m1,__m2;

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            __m1 = Menu_AppendItem(menu, "Normalize?", Menu_MyCustomItemClicked);
            __m2 = Menu_AppendItem(menu, "Geodesic?", Menu_MyCustomItemClicked);
//            __m3 = Menu_AppendItem(menu, "Go!", Menu_MyCustomItemClicked);
            
            if (_normalize == true)
            {
                __m1.CheckState = System.Windows.Forms.CheckState.Checked;
            }
            else
            {
                __m1.CheckState = System.Windows.Forms.CheckState.Unchecked;
            }
            if (_geodesic == true)
            {
                __m2.CheckState = System.Windows.Forms.CheckState.Checked;
            }
            else
            {
                __m2.CheckState = System.Windows.Forms.CheckState.Unchecked;
            }
/*            if (_go == true)
            {
                __m3.CheckState = System.Windows.Forms.CheckState.Checked;
            }
            else
            {
                __m3.CheckState = System.Windows.Forms.CheckState.Unchecked;
                t = 0;
            }*/
        }
        private bool _go = false;
        private bool _normalize = true;
        private bool _geodesic = true;
        List<string> output;
        private void Menu_MyCustomItemClicked(Object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem __m = sender as System.Windows.Forms.ToolStripMenuItem;
            if (__m == __m1)
            {
                if (__m.CheckState == System.Windows.Forms.CheckState.Checked)
                {
                    __m.CheckState = System.Windows.Forms.CheckState.Unchecked;
                    _normalize = false;
                }
                else if (__m.CheckState == System.Windows.Forms.CheckState.Unchecked)
                {
                    __m.CheckState = System.Windows.Forms.CheckState.Checked;
                    _normalize = true;
                }
            }
            if (__m == __m2)
            {
                if (__m.CheckState == System.Windows.Forms.CheckState.Checked)
                {
                    __m.CheckState = System.Windows.Forms.CheckState.Unchecked;
                    _geodesic = false;
                }
                else if (__m.CheckState == System.Windows.Forms.CheckState.Unchecked)
                {
                    __m.CheckState = System.Windows.Forms.CheckState.Checked;
                    _geodesic = true;
                }
            }
/*            if (__m == __m3)
            {
                if (__m.CheckState == System.Windows.Forms.CheckState.Checked)
                {
                    __m.CheckState = System.Windows.Forms.CheckState.Unchecked;
                    _go = false;
                    t = 0;
                }
                else if (__m.CheckState == System.Windows.Forms.CheckState.Unchecked)
                {
                    __m.CheckState = System.Windows.Forms.CheckState.Checked;
                    _go = true;
                }
            }*/
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            _normalize = false;//default value;
            reader.TryGetBoolean("Normalize?", ref _normalize);
            _geodesic = false;//default value;
            reader.TryGetBoolean("Geodesic?", ref _geodesic);
            _go = false;//default value;
            reader.TryGetBoolean("Go?", ref _go);
            return base.Read(reader);
        }
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetBoolean("Normalize?",_normalize);
            writer.SetBoolean("Geodesic?", _geodesic);
            writer.SetBoolean("Go?", false);
            return base.Write(writer);
        }

        static int t = 0;
        static List<Rhino.Geometry.GeometryBase> lS = null;
        static List<Rhino.Geometry.PointCloud> lPC = null;
        List<Guid[]> fixedPointsGuids=null;
        List<mikity.NumericalMethodHelper.objects.fixedNodes> lfN;
        System.Diagnostics.Stopwatch stw = new System.Diagnostics.Stopwatch();
        static double __dist = 0;
        vector lambda, qr, qo,dx;
        List<GH_particleSystem> lpS = null;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            stw.Stop();
            string dbg = stw.ElapsedMilliseconds.ToString();
            stw.Reset();
            double dist = 0;
            if (!DA.GetData(3, ref dist)) return;
            if (_go == false)
            {
                FriedChiken.clear();
                t = 0;
                if (fixedPointsGuids != null)
                {
                    foreach (Guid[] gs in fixedPointsGuids)
                    {
                        foreach (Guid g in gs)
                        {
                            Rhino.RhinoDoc.ActiveDoc.Objects.Delete(g, true);
                        }
                    }
                    fixedPointsGuids = null;
                }

            }
            else
            {
                double _dt = 0;
                double Mass = 0;
                if (!DA.GetData(1, ref Mass)) return;
                if (Mass < 1.0) Mass = 1.0;
                if (!DA.GetData(2, ref _dt)) return;
                double dt = Math.Pow(10, ((double)_dt / 10) - 4.8) * 2.0;
                if (t == 0)
                {
                    output = new List<string>();
                    FriedChiken.clear();
                    lpS = new List<GH_particleSystem>();
                    if (!DA.GetDataList(0, lpS)) return;
                    particleSystem[] _ps = null;
                    _ps = new particleSystem[lpS.Count];
                    for (int i = 0; i < lpS.Count; i++)
                    {
                        _ps[i] = lpS[i].Value;
                    }
                    FriedChiken.addParticleSystems(_ps);
                    if (fixedPointsGuids != null)
                    {
                        foreach (Guid[] gs in fixedPointsGuids)
                        {
                            foreach (Guid g in gs)
                            {
                                Rhino.RhinoDoc.ActiveDoc.Objects.Delete(g, true);
                            }
                        }
                    }
                    __dist = dist;

                    FriedChiken.begin();
                    if (FriedChiken.numCond > 0)
                    {
                        lambda = new vector(FriedChiken.numCond).zeros();
                        dx = new vector(FriedChiken.q.nElem);
                        qo = new vector(FriedChiken.q.nElem);
                        qr = new vector(FriedChiken.numCond).zeros();
                    }
                    lS = new List<Rhino.Geometry.GeometryBase>();
                    lPC = new List<Rhino.Geometry.PointCloud>();

                    lfN = new List<NumericalMethodHelper.objects.fixedNodes>();
                    fixedPointsGuids=new List<Guid[]>();
                    for (int i = 0; i < FriedChiken.particleSystems.Count; i++)
                    {
                        for (int j = 0; j < FriedChiken.particleSystems[i].objList.Count; j++)
                        {
                            if (FriedChiken.particleSystems[i].objList[j] is mikity.NumericalMethodHelper.objects.fixedNodes)
                            {
                                mikity.NumericalMethodHelper.objects.fixedNodes fN = (mikity.NumericalMethodHelper.objects.fixedNodes)FriedChiken.particleSystems[i].objList[j];
                                lfN.Add(fN);
                                fixedPointsGuids.Add(new Guid[fN.nNodes]);
                                Rhino.Geometry.Point3d[] ps = new Rhino.Geometry.Point3d[fN.nNodes];
                                Guid[] gs = fixedPointsGuids[fixedPointsGuids.Count-1];
                                
                                for (int k = 0; k < fN.nNodes; k++)
                                {
                                    ps[k] = new Rhino.Geometry.Point3d(fN.nodeList[k].getNodes()[0, 0]+dist, fN.nodeList[k].getNodes()[0, 1], fN.nodeList[k].getNodes()[0, 2]);
                                    gs[k] = Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(ps[k]);
                                }
                                Rhino.RhinoDoc.ActiveDoc.Groups.Add(gs);
                            }
                        }
                    }
                }
                //Computation
                stw.Start();
                FriedChiken.Tick(t);//要素アップデート、勾配の計算
                FriedChiken.Tack(t);//マスク等後処理
                if (FriedChiken.numCond > 0)
                {
                    var jacob = ShoNS.Array.DoubleArray.From(FriedChiken.getJacobian().rawData);
                    var omega = ShoNS.Array.DoubleArray.From(FriedChiken.omega.rawData);
                    jacob = jacob.T;
                    omega = omega.T;
                    var solver = new ShoNS.Array.Solver(jacob);
                    var _lambda=solver.Solve(omega);
                    _lambda.CopyTo(lambda, 0);
                    
                    FriedChiken.omega.xminusyA(FriedChiken.omega, lambda, FriedChiken.getJacobian());
                }
                string tmp="\t"+t.ToString()+"\t";
//                DA.SetData(1, FriedChiken.omega.norm);
                tmp += FriedChiken.omega.norm.ToString()+"\t";
                if (_normalize == true)
                {
                    FriedChiken.omega.dividedby(FriedChiken.omega.norm);//力を加速度に

                }

                FriedChiken.omega.MinusTo(FriedChiken.r);//力を加速度に
                
                FriedChiken.q.times(0.98).Add(dt/Mass, FriedChiken.r);
                if (_geodesic == true)
                {
                    if (FriedChiken.numCond > 0)
                    {
                        matrix.y_equals_Ax(FriedChiken.getJacobian(), FriedChiken.q, qr);
                        var jacob = ShoNS.Array.DoubleArray.From(FriedChiken.getJacobian().rawData);
                        var _qr = ShoNS.Array.DoubleArray.From(qr.rawData).T;
                        var solve = new ShoNS.Array.Solver(jacob);
                        var z=solve.Solve(_qr);
                        double[] _qo = qo.rawData;
                        z.CopyTo(_qo, 0);
                        FriedChiken.q.Subtract(qo);
                    }
                }
                FriedChiken.x.Add(dt, FriedChiken.q);
                int __s = 0;
                if (FriedChiken.numCond > 0)
                {
                    FriedChiken.Tick(t);//要素アップデート、勾配の計算
                    FriedChiken.Tack(t);//マスク等後処理

                    for (__s = 0; __s < 50; __s++)
                    {
                        var f = ShoNS.Array.DoubleArray.From(FriedChiken.getJacobian().rawData);
                        var g = ShoNS.Array.DoubleArray.From(FriedChiken.getResidual().rawData).T;
                        var solver = new ShoNS.Array.Solver(f);
                        var _dx=solver.Solve(g);
                        _dx.CopyTo(dx.rawData, 0);
                        FriedChiken.x.Subtract(1.0,dx);
                        FriedChiken.Tick(t);//要素アップデート、勾配の計算
                        FriedChiken.Tack(t);//マスク等後処理
                        if (FriedChiken.getResidual().norm < 0.001) break;
                    }
                }
                if (FriedChiken.numCond > 0)
                {
                    tmp += FriedChiken.getResidual().norm.ToString() + "\t";
                    tmp += (__s+1).ToString();
                }
                if (t <1000)
                {
                    output.Add(tmp);
                }

                //DA.SetDataList(1, output);
                stw.Stop();
                dbg = t.ToString()+"\n"+dbg + "\n" + stw.ElapsedMilliseconds.ToString();
                stw.Reset();
                
                //////////////
                for (int i = 0; i < lfN.Count; i++)
                {
                    Guid[] gs = fixedPointsGuids[i];
                    for (int j = 0; j < gs.Count(); j++)
                    {
                        Rhino.DocObjects.PointObject obj = (Rhino.DocObjects.PointObject)Rhino.RhinoDoc.ActiveDoc.Objects.Find(gs[j]);
                        Rhino.Geometry.Point3d p = new Rhino.Geometry.Point3d(obj.PointGeometry.Location.X, obj.PointGeometry.Location.Y, obj.PointGeometry.Location.Z);
                        if (lfN[i].fixX)
                        {
                            lfN[i].nodeList[j].getNodes()[0, 0] = lfN[i].nodeList[j].getNodes()[0, 0]*0.95+(p.X - dist)*0.05; //*
                            if (dist != __dist)
                            {
                                    p.X=p.X+dist-__dist;
                            }
                        }else
                        {
                                
                                p.X = lfN[i].nodeList[j].getNodes()[0, 0] + dist;
                        }
                        if (lfN[i].fixY) 
                        {
                            lfN[i].nodeList[j].getNodes()[0, 1] = lfN[i].nodeList[j].getNodes()[0, 1]*0.95+p.Y*0.05;//*
                        }else
                        {
                            p.Y = lfN[i].nodeList[j].getNodes()[0, 1];
                        }
                        if (lfN[i].fixZ)
                        {
                            lfN[i].nodeList[j].getNodes()[0, 2] = lfN[i].nodeList[j].getNodes()[0, 2]*0.95+p.Z*0.05; //*
                        }else
                        {
                            p.Z = lfN[i].nodeList[j].getNodes()[0, 2];
                        }
                        double x=0, y=0, z=0;
                        x = p.X - obj.PointGeometry.Location.X;
                        y = p.Y - obj.PointGeometry.Location.Y;
                        z = p.Z - obj.PointGeometry.Location.Z;

                        Rhino.Geometry.Transform tx = Rhino.Geometry.Transform.Translation(x,y,z);
                        gs[j] = Rhino.RhinoDoc.ActiveDoc.Objects.Transform(gs[j], tx,true);
                    }
                }
                if (dist != __dist)
                {
                    __dist = dist;
                }
                

                foreach (GH_particleSystem pS in lpS)
                {
                    if (pS != null)
                    {
                        if (pS.UPGR != null)
                        {
                            pS.UPGR(dist,0,0);
                        }
                    }
                }
                
                
                DA.SetData(1, dbg);
                stw.Start();
                t++;
            }

            return;
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("925d9ba4-74a2-4a55-a02a-0a7bb5cc7919"); }
        }
    }
    
}
