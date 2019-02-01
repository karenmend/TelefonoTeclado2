using System;
using System.Windows;
using Microsoft.Win32;
using NAudio.Wave;
using System.Linq;

namespace GraficadorSeñales
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double amplitudMaxima = 1;
        Señal señal;
        Señal señalResultado;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnGraficar_Click(object sender, RoutedEventArgs e)
        {

            var reader = new AudioFileReader(txtRutaArchivo.Text);

            double tiempoInicial = 0;
            double tiempoFinal = reader.TotalTime.TotalSeconds;
            double frecuenciaMuestreo = reader.WaveFormat.SampleRate;

            txtFrecuenciaMuestreo.Text = frecuenciaMuestreo.ToString();
            txtTiempoInicial.Text = "0";
            txtTiempoFinal.Text = tiempoFinal.ToString();

            señal = new SeñalPersonalizada();

            //---------------------------------PRIMERA SEÑAL------------------------------------------------------//
            señal.TiempoInicial = tiempoInicial;
            señal.TiempoFinal = tiempoFinal;
            señal.FrecuenciaMuestreo = frecuenciaMuestreo;

            //Construir nuestra señal a traves del archivo de audio
            var bufferLectura = new float[reader.WaveFormat.Channels];
            int muestrasLeidas = 1;
            double instanteActual = 0;
            double intervaloMuestra = 1.0 / frecuenciaMuestreo;

            do
            {
                muestrasLeidas = reader.Read(bufferLectura, 0, reader.WaveFormat.Channels);
                if (muestrasLeidas > 0)
                {
                    double max = bufferLectura.Take(muestrasLeidas).Max();
                    señal.Muestras.Add(new Muestra(instanteActual, max));
                }
                instanteActual += intervaloMuestra;
            } while (muestrasLeidas > 0);

            
            señal.actualizarAmplitudMaxima();
            
            amplitudMaxima = señal.AmplitudMaxima;
           
            plnGrafica.Points.Clear();
            
            lblAmplitudMaximaY.Text = amplitudMaxima.ToString("F");
            lblAmplitudMaximaNegativaY.Text = "-" + amplitudMaxima.ToString("F");

            //PRIMERA SEÑAL
            if (señal != null)
            {
                //Recorre todos los elementos de una coleccion o arreglo
                foreach (Muestra muestra in señal.Muestras)
                {
                    plnGrafica.Points.Add(new Point((muestra.X - tiempoInicial) * scrContenedor.Width, (muestra.Y /
                        amplitudMaxima * ((scrContenedor.Height / 2.0) - 30) * -1) + 
                        (scrContenedor.Height / 2)));

                }
                
            }

            
            plnEjeX.Points.Clear();
            //Punto del principio
            plnEjeX.Points.Add(new Point(0, (scrContenedor.Height / 2)));
            //Punto del final
            plnEjeX.Points.Add(new Point((tiempoFinal - tiempoInicial) * scrContenedor.Width, 
                (scrContenedor.Height / 2)));

            plnEjeY.Points.Clear();
            //Punto del principio
            plnEjeY.Points.Add(new Point((0 - tiempoInicial) * scrContenedor.Width , (señal.AmplitudMaxima * 
                ((scrContenedor.Height / 2.0) - 30) * -1) + (scrContenedor.Height / 2)));
            //Punto del final
            plnEjeY.Points.Add(new Point((0 - tiempoInicial) * scrContenedor.Width, (-señal.AmplitudMaxima * 
                ((scrContenedor.Height / 2.0) - 30) * -1) + (scrContenedor.Height / 2)));                     
        }
        
        private void btnTransformadaFourier_Click(object sender, RoutedEventArgs e)
        {
            Señal transformada = Señal.transformar(señal);
            transformada.actualizarAmplitudMaxima();

            plnGraficaResultado.Points.Clear();

            lblAmplitudMaximaY_Resultado.Text = transformada.AmplitudMaxima.ToString("F");
            lblAmplitudMaximaNegativaY_Resultado.Text = "-" + transformada.AmplitudMaxima.ToString("F");

            //PRIMERA SEÑAL
            if (transformada != null)
            {
                //Recorre todos los elementos de una coleccion o arreglo
                foreach (Muestra muestra in transformada.Muestras)
                {
                    plnGraficaResultado.Points.Add(new Point((muestra.X - transformada.TiempoInicial) * scrContenedor_Resultado.Width, (muestra.Y /
                        transformada.AmplitudMaxima * ((scrContenedor_Resultado.Height / 2.0) - 30) * -1) +
                        (scrContenedor_Resultado.Height / 2)));
                }

                int indiceMinimoFrecuenciasBajas = 0;
                int indiceMaximoFrecuenciasBajas = 0;
                int indiceMinimoFrecuenciasAltas = 0;
                int indiceMaximoFrecuenciasAltas = 0;

                indiceMinimoFrecuenciasBajas = 680 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;
                indiceMaximoFrecuenciasBajas = 1000 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;
                indiceMinimoFrecuenciasAltas = 1200 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;
                indiceMaximoFrecuenciasAltas = 1500 * transformada.Muestras.Count / (int)señal.FrecuenciaMuestreo;

                double valorMaximo = 0;
                int indiceMaximo = 0;
                int indiceActual = 0;

                double valorMaximo2 = 0;
                int indiceMaximo2 = 0;
                int indiceActual2 = 0;


                for (indiceActual2 = indiceMinimoFrecuenciasAltas; indiceActual2 < indiceMaximoFrecuenciasAltas; indiceActual2++)
                {
                    if (transformada.Muestras[indiceActual2].Y > valorMaximo2)
                    {
                        valorMaximo2 = transformada.Muestras[indiceActual2].Y;
                        indiceMaximo2 = indiceActual2;

                    }

                }



                for (indiceActual = indiceMinimoFrecuenciasBajas; indiceActual < indiceMaximoFrecuenciasBajas; indiceActual++)
                {
                    if (transformada.Muestras[indiceActual].Y > valorMaximo)
                    {
                        valorMaximo = transformada.Muestras[indiceActual].Y;
                        indiceMaximo = indiceActual;

                    }

                }



               
                double frecuenciaFundamental = (double)indiceMaximo * señal.FrecuenciaMuestreo / (double)transformada.Muestras.Count;
                Hertz.Text = frecuenciaFundamental.ToString() + "Hz";

                double frecuenciaFundamental2 = (double)indiceMaximo2 * señal.FrecuenciaMuestreo / (double)transformada.Muestras.Count;
                Hertz2.Text = frecuenciaFundamental2.ToString() + "Hz";


                if (frecuenciaFundamental >= 937 && frecuenciaFundamental <= 945)
                {
                    if (frecuenciaFundamental2 >= 1206 && frecuenciaFundamental2 <= 1212)
                    {
                        tecla.Text = "Es la tecla *.";
                    }

                    if (frecuenciaFundamental2 >= 1333 && frecuenciaFundamental2 <= 1339)
                    {
                        tecla.Text = "Es la tecla 0.";
                    }
                    if (frecuenciaFundamental2 >= 1444 && frecuenciaFundamental2 <= 1480)
                    {
                        tecla.Text = "Es la tecla #.";
                    }

                }

                if (frecuenciaFundamental >= 849 && frecuenciaFundamental <= 855)
                {
                    if (frecuenciaFundamental2 >= 1206 && frecuenciaFundamental2 <= 1212)
                    {
                        tecla.Text = "Es la tecla 7.";
                    }

                    if (frecuenciaFundamental2 >= 1333 && frecuenciaFundamental2 <= 1339)
                    {
                        tecla.Text = "Es la tecla 8.";
                    }
                    if (frecuenciaFundamental2 >= 1444 && frecuenciaFundamental2 <= 1480)
                    {
                        tecla.Text = "Es la tecla 9.";
                    }

                }

                if (frecuenciaFundamental >= 767 && frecuenciaFundamental <= 773)
                {
                    if (frecuenciaFundamental2 >= 1206 && frecuenciaFundamental2 <= 1212)
                    {
                        tecla.Text = "Es la tecla 4.";
                    }

                    if (frecuenciaFundamental2 >= 1333 && frecuenciaFundamental2 <= 1339)
                    {
                        tecla.Text = "Es la tecla 5.";
                    }
                    if (frecuenciaFundamental2 >= 1444 && frecuenciaFundamental2 <= 1480)
                    {
                        tecla.Text = "Es la tecla 6.";
                    }

                }

                if (frecuenciaFundamental >= 694 && frecuenciaFundamental <= 700)
                {
                    if (frecuenciaFundamental2 >= 1206 && frecuenciaFundamental2 <= 1212)
                    {
                        tecla.Text = "Es la tecla 1.";
                    }

                    if (frecuenciaFundamental2 >= 1333 && frecuenciaFundamental2 <= 1339)
                    {
                        tecla.Text = "Es la tecla 2.";
                    }
                    if (frecuenciaFundamental2 >= 1444 && frecuenciaFundamental2 <= 1480)
                    {
                        tecla.Text = "Es la tecla 3.";
                    }

                }

            }


                plnEjeXResultado.Points.Clear();
            //Punto del principio
            plnEjeXResultado.Points.Add(new Point(0, (scrContenedor_Resultado.Height / 2)));
            //Punto del final
            plnEjeXResultado.Points.Add(new Point((transformada.TiempoFinal - transformada.TiempoInicial) * scrContenedor_Resultado.Width,
                (scrContenedor_Resultado.Height / 2)));

            plnEjeYResultado.Points.Clear();
            //Punto del principio
            plnEjeYResultado.Points.Add(new Point((0 - transformada.TiempoInicial) * scrContenedor_Resultado.Width, (transformada.AmplitudMaxima *
                ((scrContenedor_Resultado.Height / 2.0) - 30) * -1) + (scrContenedor_Resultado.Height / 2)));
            //Punto del final
            plnEjeYResultado.Points.Add(new Point((0 - transformada.TiempoInicial) * scrContenedor_Resultado.Width, (-transformada.AmplitudMaxima *
                ((scrContenedor_Resultado.Height / 2.0) - 30) * -1) + (scrContenedor_Resultado.Height / 2)));
        }

        private void btnExaminar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
          
            if ((bool)fileDialog.ShowDialog())
            {
                txtRutaArchivo.Text = fileDialog.FileName;
            }
        }
    }

}
