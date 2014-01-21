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
using ShoNS.Array;
namespace mikity.ghComponents
{

    public class GH_FriedChikenMainLoop : Grasshopper.Kernel.GH_Component
    {
        public static bool DEV = true;
        Func<double, double> Drift0 = (v) => { return 0.98; };
        Func<double, double> Drift1 = (v) => { /*if (v > 0)*/ return v / 20d + 0.95; /*else return 0.95;*/ };

        Func<double, double> Drift2 = (v) => { if (v >=0)return 1.0; else return 0.0; };
        Func<double, double> Drift3 = (v) => { return 1.0; };

        mikity.visualize.fullScreen full;
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

        public override void AddedToDocument(Grasshopper.Kernel.GH_Document document)
        {

            base.AddedToDocument(document);

            //Rhino.RhinoDoc.ReplaceRhinoObject += RhinoDoc_ReplaceRhinoObject;
            timer = new System.Windows.Forms.Timer();
            timer.Tick += timer_Tick;
            timer.Enabled = false;
            timer.Interval = 1;

            // register evens
            keyboardHook.KeyDown = new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyDown);
            keyboardHook.KeyUp = new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyUp);
            keyboardHook._activate = new RamGecTools.KeyboardHook.activate(activate);
            keyboardHook._deactivate = new RamGecTools.KeyboardHook.deactivate(deactivate);
            keyboardHook.Uninstall();
            keyboardHook.Install();
            full = new mikity.visualize.fullScreen();
            full.deactivate();
            full.Show();
            full.resetGo();
            full.drift1();
            full.renewPlot(Drift1);
            full.onRF();
            full.onNorm();
            full.onGeo();
            full.offVN();
            full.offIF();
        }
        
        public override void RemovedFromDocument(Grasshopper.Kernel.GH_Document document)
        {
            base.RemovedFromDocument(document);
            keyboardHook.Uninstall();
            keyboardHook.KeyDown -= new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyDown);
            keyboardHook.KeyUp -= new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyUp);
            if (full != null)
            {
                full.Close();
                full = null;
            }
            if (timer != null)
            {
                timer = null;
            }
        }

        public override void DocumentContextChanged(Grasshopper.Kernel.GH_Document document, Grasshopper.Kernel.GH_DocumentContext context)
        {
            base.DocumentContextChanged(document, context);
            if (context == Grasshopper.Kernel.GH_DocumentContext.Loaded)
            {

                //Rhino.RhinoDoc.ReplaceRhinoObject += RhinoDoc_ReplaceRhinoObject;

                timer = new System.Windows.Forms.Timer();
                timer.Tick += timer_Tick;
                timer.Enabled = false;
                timer.Interval = 1;

                // register evens

                keyboardHook.KeyDown = new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyDown);
                keyboardHook.KeyUp = new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyUp);
                keyboardHook._activate = new RamGecTools.KeyboardHook.activate(activate);
                keyboardHook._deactivate = new RamGecTools.KeyboardHook.deactivate(deactivate);
                keyboardHook.Uninstall();

                keyboardHook.Install();
                full = new mikity.visualize.fullScreen(DEV);
                full.deactivate();
                full.Show();
                full.resetGo();
                full.drift1();
                full.renewPlot(Drift1);
                full.onRF();
                full.onNorm();
                full.onGeo();
                full.offVN();
                full.offIF();
            }
            if (context == Grasshopper.Kernel.GH_DocumentContext.Unloaded)
            {

                keyboardHook.Uninstall();
                keyboardHook.KeyDown -= new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyDown);
                keyboardHook.KeyUp -= new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyUp);
                if (full != null)
                {
                    full.Close();
                    full = null;
                }
            }
        }
        void activate()
        {
            full.activate();
        }
        void deactivate()
        {
            full.deactivate();
        }
       
        RamGecTools.MouseHook mouseHook = new RamGecTools.MouseHook();
        RamGecTools.KeyboardHook keyboardHook = new RamGecTools.KeyboardHook();
        System.Random rand = new Random();
        bool keyboardHook_KeyUp(RamGecTools.KeyboardHook.VKeys key)
        {
            if (DEV)
            {
                if (key == RamGecTools.KeyboardHook.VKeys.KEY_F)
                {

                    for (int i = 0; i < FriedChiken.nParticles; i++)
                    {
                        FriedChiken.x[i * 3 + 0] += (rand.NextDouble() - 0.5) * 50d;
                        FriedChiken.x[i * 3 + 1] += (rand.NextDouble() - 0.5) * 50d;
                        FriedChiken.x[i * 3 + 2] += (rand.NextDouble() - 0.5) * 50d;
                    }
                    return true;
                }

                if (key == RamGecTools.KeyboardHook.VKeys.KEY_E)
                {
                    if (_IF)
                    {
                        _IF = false;
                        full.offIF();
                    }
                    else
                    {
                        _IF = true;
                        full.onIF();
                    }
                    return true;
                }
                if (key == RamGecTools.KeyboardHook.VKeys.KEY_R)
                {
                    if (_RP)
                    {
                        _RP = false;
                        full.offRF();
                    }
                    else
                    {
                        _RP = true;
                        full.onRF();
                    }
                    return true;
                }

                if (key == RamGecTools.KeyboardHook.VKeys.KEY_N)
                {
                    if (_normalize)
                    {
                        _normalize = false;
                        full.offNorm();
                    }
                    else
                    {
                        _normalize = true;
                        full.onNorm();
                    }
                    return true;
                }

                if (key == RamGecTools.KeyboardHook.VKeys.KEY_H)
                {
                    if (_geodesic)
                    {
                        _geodesic = false;
                        full.offGeo();
                    }
                    else
                    {
                        _geodesic = true;
                        full.onGeo();
                    }
                    return true;
                }

                if (key == RamGecTools.KeyboardHook.VKeys.KEY_A)
                {
                    if (_drift1)
                    {
                        _drift1 = false;
                        _drift2 = true;
                        _drift3 = false;
                        full.drift2();
                        full.renewPlot(Drift2);
                    }
                    else if (_drift2)
                    {
                        _drift1 = false;
                        _drift2 = false;
                        _drift3 = true;
                        full.drift3();
                        full.renewPlot(Drift3);
                    }
                    else if (_drift3)
                    {
                        _drift1 = false;
                        _drift2 = false;
                        _drift3 = false;
                        full.drift0();
                        full.renewPlot(Drift0);
                    }
                    else
                    {
                        _drift1 = true;
                        _drift2 = false;
                        _drift3 = false;
                        full.drift1();
                        full.renewPlot(Drift1);
                    }
                    return true;
                }

            }

            if (key == RamGecTools.KeyboardHook.VKeys.ESCAPE)
            {
                full.resetGo();
                full.clearNorm();
                _go = false;
                timer.Enabled = false;
                if (t!=0&&refX != null)
                {
                    t = 0;
                    for (int i = 0; i < FriedChiken.x.rawData.Length; i++)
                    {
                        FriedChiken.x.rawData[i] = refX[i];
                    }
                    FriedChiken.Tick(t);//要素アップデート、勾配の計算
                    FriedChiken.Tack(t);//マスク等後処理
                    refX = null;
                }
                ExpireSolution(true);
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_G)
            {
                if (_go)
                {
                    full.pauseGo();

                    _go = false;
                    timer.Enabled = false;
                }
                else
                {
                    full.onGo();

                    _go = true;
                    timer.Enabled = true;
                }
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_T)
            {
                if (_VN)
                {
                    _VN = false;
                    full.offVN();
                }
                else
                {
                    _VN = true;
                    full.onVN();
                }
                return true;
            }
            return false;
        }

        bool keyboardHook_KeyDown(RamGecTools.KeyboardHook.VKeys key)
        {
            if (DEV)
            {
                if (key == RamGecTools.KeyboardHook.VKeys.KEY_E)
                {
                    return true;
                }
                if (key == RamGecTools.KeyboardHook.VKeys.KEY_F)
                {
                    return true;
                }
                if (key == RamGecTools.KeyboardHook.VKeys.KEY_R)
                {
                    return true;
                }
                if (key == RamGecTools.KeyboardHook.VKeys.KEY_N)
                {
                    return true;
                }
                if (key == RamGecTools.KeyboardHook.VKeys.KEY_H)
                {
                    return true;
                }

                if (key == RamGecTools.KeyboardHook.VKeys.KEY_A)
                {
                    return true;
                }
            }
            if (key == RamGecTools.KeyboardHook.VKeys.ESCAPE)
            {
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_G)
            {
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_T)
            {
                return true;
            }
            return false;
        }
        void timer_Tick(object sender, EventArgs e)
        {
            this.ExpireSolution(true);
        }
        
        System.Windows.Forms.Timer timer;
        
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ParticleSystems", "pS", "ParticleSystems", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddNumberParameter("Distance", "D", "Distance from the original model", Grasshopper.Kernel.GH_ParamAccess.item, 10.0);
        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
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
            if(_VN)
            {
                var x=FriedChiken.getParticles();
                for (int i = 0; i < FriedChiken.nParticles; i++)
                {
                    Rhino.DocObjects.ObjectAttributes a2 = att.Duplicate();
                    a2.LayerIndex = 2;
                    obj_ids.Add(doc.Objects.AddTextDot(new Rhino.Geometry.TextDot(i.ToString("000"),new Rhino.Geometry.Point3d(x[i, 0] + __dist, x[i, 1], x[i, 2])),a2));
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
                    var x=FriedChiken.getParticles();
                    if (_VN)
                    {
                        for (int i = 0; i < FriedChiken.nParticles; i++)
                        {
                            args.Display.Draw2dText(i.ToString("000"), System.Drawing.Color.Red, new Rhino.Geometry.Point3d(x[i, 0] + __dist, x[i, 1], x[i, 2]), true);
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
        private bool _drift1 = true, _drift2 = false, _drift3 = false, _RP = true, _IF = false;
        private bool _normalize = true;
        private bool _geodesic = true;
        private bool _VN = false;
        List<string> output;
        private void phi()
        {
            var jacob = ShoNS.Array.DoubleArray.From(FriedChiken.getJacobian().rawData);
            var omega = ShoNS.Array.DoubleArray.From(FriedChiken.omega.rawData);
            jacob = jacob.T;
            omega = omega.T;
            var solver = new ShoNS.Array.Solver(jacob);
            var _lambda = solver.Solve(omega);
            _lambda.CopyTo(lambda, 0);
            FriedChiken.omega.xminusyA(FriedChiken.omega, lambda, FriedChiken.getJacobian());

        }
        private void varphi()
        {
            double norm = FriedChiken.q.norm;                            
            matrix.y_equals_Ax(FriedChiken.getJacobian(), FriedChiken.q, qr);
            var jacob = ShoNS.Array.DoubleArray.From(FriedChiken.getJacobian().rawData);
            var _qr = ShoNS.Array.DoubleArray.From(qr.rawData).T;
            var solve = new ShoNS.Array.Solver(jacob);
            var z = solve.Solve(_qr);
            double[] _qo = qo.rawData;
            z.CopyTo(_qo, 0);
            FriedChiken.q.Subtract(qo);
            if (FriedChiken.q.norm != 0)
            {
                FriedChiken.q.dividedby(FriedChiken.q.norm);
                FriedChiken.q.times(norm);
            }
        }

        private int psi()
        {
            FriedChiken.Tick(t);//要素アップデート、勾配の計算
            FriedChiken.Tack(t);//マスク等後処理
            int itr = 0;
            for (int s = 0; s < 50; s++)
            {
                var ff = ShoNS.Array.DoubleArray.From(FriedChiken.getJacobian().rawData);
                var g = ShoNS.Array.DoubleArray.From(FriedChiken.getResidual().rawData).T;
                var solver = new ShoNS.Array.Solver(ff);
                var _dx = solver.Solve(g);
                _dx.CopyTo(dx.rawData, 0);
                FriedChiken.x.Subtract(1.0, dx);
                FriedChiken.Tick(t);//要素アップデート、勾配の計算
                FriedChiken.Tack(t);//マスク等後処理
                itr++;
                if (FriedChiken.getResidual().norm < 0.0001) break; 
            }
            return itr;
        }
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
            return base.Read(reader);
        }
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
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
        DoubleArray refX = null;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            stw.Stop();
            string dbg = stw.ElapsedMilliseconds.ToString();
            stw.Reset();
            double dist = 0;
            if (!DA.GetData(1, ref dist)) return;
            if (_go == false)
            {
                t = 0;
                FriedChiken.clear();
                lpS = new List<GH_particleSystem>();

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

                if (!DA.GetDataList(0, lpS)) return;
                particleSystem[] _ps = null;
                _ps = new particleSystem[lpS.Count];
                for (int i = 0; i < lpS.Count; i++)
                {
                    _ps[i] = lpS[i].Value;
                }
                FriedChiken.addParticleSystems(_ps);
                //FriedChiken.begin();
                __dist = dist;
                lS = new List<Rhino.Geometry.GeometryBase>();
                lPC = new List<Rhino.Geometry.PointCloud>();

                lfN = new List<NumericalMethodHelper.objects.fixedNodes>();
                fixedPointsGuids=new List<Guid[]>();

                foreach (GH_particleSystem pS in lpS)
                {
                    if (pS != null)
                    {
                        if (pS.UPGR != null)
                        {
                            pS.UPGR(dist, 0, 0);
                        }
                    }
                }
            }
            else
            {
                double dt = full.getDt();
                if (t == 0)
                {
                    output = new List<string>();
                    //firstAction(DA);
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
                    FriedChiken.begin();
                    refX = DoubleArray.From(FriedChiken.x.rawData);
                    __dist = dist;
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

                while (stw.ElapsedMilliseconds< 25)
                {
                    stw.Start();
                    FriedChiken.Tick(t);//要素アップデート、勾配の計算
                    FriedChiken.Tack(t);//マスク等後処理
                    if (_IF)
                    {
                        FriedChiken.omega.zeros();
                    }
                    double resid = FriedChiken.getResidual().norm;
                    
                    if (FriedChiken.numCond > 0) phi();
                    string tmp = "\t" + t.ToString() + "\t";
                    tmp += FriedChiken.omega.norm.ToString() + "\t";

                    if (_geodesic) if (FriedChiken.numCond > 0) varphi();
                    var v = DoubleArray.From(FriedChiken.q.rawData);
                    double normW=FriedChiken.omega.norm;
                    if (_normalize == true)
                    {
                        if(normW!=0)
                            FriedChiken.omega.dividedby(normW);//力を正規化
                    }
                    var a = DoubleArray.From(FriedChiken.omega.rawData);

                    FriedChiken.omega.MinusTo(FriedChiken.r);//力を加速度に
                    double norm1 = (v * v.T)[0, 0];
                    double norm2 = (v * a.T)[0, 0];
                    double norm3 = (a * a.T)[0, 0];
                    double f = 0;
                    if (norm1 * norm3 != 0)
                    {
                        f = -norm2 / Math.Sqrt(norm1 * norm3);
                    }
                    else
                    {
                        f = 1;
                    }

                    double damping = 0;
                    if (_drift1)
                    {
                        damping = Drift1(f);
                    }
                    else if (_drift2)
                    {
                        damping = Drift2(f);
                    }
                    else if (_drift3)
                    {
                        damping = Drift3(f);
                    }
                    else
                    {
                        damping = Drift0(f);
                    }
                    full.move(f);
                    dbg = "damping:" + damping      .ToString() + "\n" + "dt:" + dt.ToString() + "\n" + "Step#:"+t.ToString();
                    full.setDbgText(dbg);
                    FriedChiken.q.times(damping).Add(dt, FriedChiken.r);
                    double normQ = FriedChiken.q.norm;
                    double K = normQ * normQ * 0.5;
                    double P = FriedChiken.energy;
                    double E = K + P;
                    int itr = 0;
                    FriedChiken.x.Add(dt, FriedChiken.q);
                    if (FriedChiken.numCond > 0) itr=psi();
                    full.addNorm(K, E,itr,normW,resid);

                    stw.Stop();
                    t++;
                    if (_RP == false) break;
                }
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
            }

            return;
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("925d9ba4-74a2-4a55-a02a-0a7bb5cc7919"); }
        }
    }
    
}
