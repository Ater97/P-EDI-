﻿using Btree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Proyecto_microSQL.Utilidades
{
    class Utilities
    {
        string path;
        List<string> palabrasReservadas;
        List<string> palabrasReemplazo;
        List<string> tiposReservados;
        List<string> tiposReemplazo;
        Queue<CrearTabla> tablasPorCrear = new Queue<CrearTabla>();

        public void setPath(string p)
        {
            path = p;
        }

        #region CreateStuff
        public void crearFolder()
        {
            Directory.CreateDirectory(path);

            Directory.CreateDirectory(path + "\\microSQL");
            Directory.CreateDirectory(path + "\\tablas");
        }
        //string path = @"Archivo\microSQL.ini";

        public bool CrearDefault()
        {
            //  path = Path.Combine(path, "microSQL\\microSQL.ini"); 
            try
            {
                FileStream fs = File.Create(path + "microSQL\\microSQL.ini");
                fs.Close();
                using (StreamWriter file = new StreamWriter(path + "microSQL\\microSQL.ini", true))
                {
                    file.WriteLine("SELECT,SELECT");
                    file.WriteLine("FROM,FROM");
                    file.WriteLine("DELETE,DELETE");
                    file.WriteLine("WHERE,WHERE");
                    file.WriteLine("CREATE TABLE,CREATE TABLE");
                    file.WriteLine("DROP TABLE,DROP TABLE");
                    file.WriteLine("INSERT INTO,INSERT INTO");
                    file.WriteLine("VALUES,VALUES");
                    file.WriteLine("GO,GO");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<string> CargarComando()
        {
            try
            {

                string[] line;
                List<string> comandolst = new List<string>();

                palabrasReservadas = new List<string>();
                palabrasReemplazo = new List<string>();

                string[] lines = File.ReadAllLines(path + "microSQL\\microSQL.ini");
                for (int i = 0; i < lines.Length; i++)
                {
                    line = lines[i].Split(',');
                    palabrasReservadas.Add(line[1]);
                    palabrasReemplazo.Add(string.Join("", line[1].Split(' ')));
                }

                return palabrasReservadas;
            }
            catch
            {
                return null;
            }
        }

        public List<string> CargarTiposDefault()
        {
            tiposReservados = new List<string>();
            tiposReemplazo = new List<string>();

            tiposReservados.Add("INT PRIMARY KEY");
            tiposReservados.Add("VARCHAR(100)");
            tiposReservados.Add("DATETIME");
            tiposReservados.Add("INT");

            tiposReemplazo.Add("INTPRIMARYKEY");
            tiposReemplazo.Add("VARCHAR(100)");
            tiposReemplazo.Add("DATETIME");
            tiposReemplazo.Add("INT");

            return tiposReservados;
        }

        public List<string> ObtenerReemplazo()
        {
            return palabrasReemplazo;
        }

        public bool CrearArchivoTabla(string id, List<string> columns, string tablename)
        {
            try
            {
                FileStream fs = File.Create(path + "tablas\\" + tablename + ".tabla");
                fs.Close();
                using (StreamWriter file = new StreamWriter(path + "tablas\\" + tablename + ".tabla", true))
                {
                    file.WriteLine(id + "," + string.Join(",", columns));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CrearArbol(string treeName, string id, List<string> types)
        {
            try
            {

                //****Moficar Dato[0].ToString en fabrica****
                BTree<int, standardObject> tree = new BTree<int, standardObject>(treeName, 5);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void crearTabla(CrearTabla tabla)
        {
            CrearArchivoTabla(tabla.Id, tabla.Names, tabla.TableName.Trim());
            CrearArbol(tabla.TableName.Trim(), tabla.Id, tabla.Types);
        }

        public Queue<CrearTabla> TablasPorCrear
        {
            get
            {
                return tablasPorCrear;
            }

            set
            {
                tablasPorCrear = value;
            }
        }

        #endregion

        #region Errores De Sintaxis

        public int VerificarSintaxisCrearTabla(List<string> datos)
        {
            
            if(!datos.Contains(string.Join("", tiposReservados[0].Split(' '))))
            {
                return 8;
            }

            //Verificar la existencia de la llave de apertura.
            if( datos[1] != "{" && !datos.Contains("{"))
            {
                //Error de llave de apertura no encontrado
                return 3;
            }

            //En caso que exista algo que no es el operador {
            if(datos[1] != "{")
            {
                //Error de espacios de los nombres de las variables
                return 9;
            }

            //En caso el ultimo elemento no es la llave de cierre
            if(datos[datos.Count - 1] != "}" && !datos.Contains("}"))
            {
                return 4;
            }

            //En caso que exista la llave de cierre pero hay mas elementos despues de esto.
            if(datos[datos.Count - 1] != "}")
            {
                return 1;
            }

            CrearTabla nuevaTabla = new CrearTabla();
            int[] counts = new int[3];
            nuevaTabla.TableName = datos[0];

            bool flag = false;

            for(int i = 2; i < datos.Count - 2; i++)
            {
                flag = false;

                //Omitiendo el nombre y la llave de apertura
                for(int j = 0; j < tiposReemplazo.Count; j++)
                {
                    if (datos[i] == tiposReemplazo[j])
                    {
                        return 10;
                    }

                    if(datos[i + 1] == tiposReemplazo[j])
                    {
                        flag = true;
                    }
                }

                if(flag)
                {
                    if(datos[i + 1] == tiposReemplazo[0])
                    {
                        if(nuevaTabla.Id == string.Empty)
                        {
                            nuevaTabla.Id = datos[i];                           
                        }
                        else
                        {
                            //Sobrepaso la cantidad de elementos admitidos del tipo de dato
                            nuevaTabla = null;
                            return 12;
                        }                      
                    }

                    if(datos[i + 1] == tiposReemplazo[1])
                    {
                        if(counts[0] < 3)
                        {
                            counts[0]++;
                            nuevaTabla.Names.Add(datos[i] + " " + tiposReservados[1]);
                            nuevaTabla.Types.Add(tiposReservados[1]);
                        }
                        else
                        {
                            //Sobrepaso la cantidad de elementos admitidos del tipo de dato
                            nuevaTabla = null;
                            return 13;
                        }
  
                    }

                    if (datos[i + 1] == tiposReemplazo[2])
                    {
                        if(counts[1] < 3)
                        {
                            counts[1]++;
                            nuevaTabla.Names.Add(datos[i] + " " + tiposReservados[2]);
                            nuevaTabla.Types.Add(tiposReservados[2]);
                        }
                        else
                        {
                            //Sobrepaso la cantidad de elementos admitidos del tipo de dato
                            nuevaTabla = null;
                            return 13;
                        }                      
                    }

                    if(datos[i +1] == tiposReemplazo[3] && counts[2] < 3)
                    {
                        if(counts[2] < 3)
                        {
                            counts[2]++;
                            nuevaTabla.Names.Add(datos[i] + " " + tiposReservados[3]);
                            nuevaTabla.Types.Add(tiposReservados[3]);
                        }
                        else
                        {
                            //Sobrepaso la cantidad de elementos admitidos del tipo de dato
                            nuevaTabla = null;
                            return 13;
                        }                    
                    }                   
                }
                else
                {
                    //Error de tipo de dato
                    return 11;
                }

                i++;
            }

            TablasPorCrear.Enqueue(nuevaTabla);

            return 0;
        }

        public int VerificiarSintaxisSelect()
        {


            return 0;
        }

        public int VerificiarSintaxisDelete()
        {
            return 0;
        }

        public int VerificarSintaxisDropTable()
        {
            return 0;
        }

        public int VerificarSintaxisInsertTo()
        {

            return 0;
        }


        #endregion

        public Tuple<List<string>, bool> splitArray(string[] complete, int index)
        {
            bool fg = false;
            List<string> newLines = new List<string>();
            for (int i = index; i < complete.Count(); i++)
            {
                if (complete[i].Trim() == ")" || complete[i].Trim() == "}" || complete[i].Trim() == ">" || complete[i].Trim() == "]")
                {
                    fg = true;
                    break;
                }
                newLines.Add(complete[i]);
            }
            return Tuple.Create(newLines, fg);
        }

        public int getSplitIndex(string[] completelns, int startindex, string comando)
        {
            for (int i = startindex; i < completelns.Count(); i++)
            {
                if (completelns[i].Contains(comando))
                    return i + 2;
            }
            return 0;
        }

        public string[] LimiarArray(string[] Lines, string[] toRemove)
        {
            var charsToRemove = toRemove; //eliminar caracteres extra
            for (int i = 0; i < Lines.Length; i++)
            {
                foreach (var c in charsToRemove)
                {
                    Lines[i] = Lines[i].Replace(c, string.Empty);
                }
            }

            Lines = Lines.Where(x => !string.IsNullOrEmpty(x)).ToArray(); // eliminar espacios en blanco

            return Lines;
        }

        #region Insert
        public bool insertarArchivoTabla(string tableName, List<string> values)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(path + "tablas\\" + tableName + ".tabla", true))
                {
                    file.WriteLine(string.Join(",", values));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool insertarArbol(string tableName, List<string> values, List<string> columns)
        {
            try
            {
                bool[,] c = new bool[3, 3];
                standardObject newobj = (new standardObject
                {
                    ID = int.Parse(values[0]),
                });

                var charsToRemove = new string[] { "'" };
                for (int i = 0; i < values.Count; i++)
                {
                    foreach (var ch in charsToRemove)
                    {
                        values[i] = values[i].Replace(ch, string.Empty);
                    }
                }

                string data = File.ReadLines(path + "tablas\\" + tableName + ".tabla").First();
                string[] strDataType = data.Split(',');

                for (int i = 0; i < columns.Count(); i++)
                {
                    if (strDataType[i].Contains("(INT)"))
                    {
                        if (!c[0, 0])
                        {
                            newobj.MyProperty_int1 = int.Parse(values[i]);
                            c[0, 0] = true;
                        }
                        else if (!c[0, 1])
                        {
                            newobj.MyProperty_int2 = int.Parse(values[i]);
                            c[0, 1] = true;
                        }
                        else if (!c[0, 2])
                        {
                            newobj.MyProperty_int3 = int.Parse(values[i]);
                            c[0, 2] = true;
                        }
                        else
                            return false;
                    }
                    else if (strDataType[i].Contains("(VARCHAR(100))"))
                    {
                        if (!c[1, 0])
                        {
                            newobj.MyProperty_vchar1 = values[i];
                            c[1, 0] = true;
                        }
                        else if (!c[1, 1])
                        {
                            newobj.MyProperty_vchar2 = values[i];
                            c[1, 1] = true;
                        }
                        else if (!c[1, 2])
                        {
                            newobj.MyProperty_vchar3 = values[i];
                            c[1, 2] = true;
                        }
                        else
                            return false;
                    }
                    else if (strDataType[i].Contains("(DATETIME)"))
                    {
                        if (!c[2, 0])
                        {
                            newobj.MyProperty_dt1 = DateTime.Parse(values[i]);
                            c[2, 0] = true;
                        }
                        else if (!c[2, 1])
                        {
                            newobj.MyProperty_dt2 = DateTime.Parse(values[i]);
                            c[2, 1] = true;
                        }
                        else if (!c[2, 2])
                        {
                            newobj.MyProperty_dt3 = DateTime.Parse(values[i]);
                            c[2, 2] = true;
                        }
                        else
                            return false;
                    }
                }

                //****Encontrar como cargar arbol desde archivo****
                BTree<int, standardObject> tree = new BTree<int, standardObject>(tableName, 5);
                // BTree<int, standardObject> tree = new BTree<int, standardObject>(tableName);

                tree.Insertar(int.Parse(values[0]), newobj);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Insertar(string tableName, List<string> columns, List<string> values)
        {
            for (int i = 0; i < columns.Count(); i++)//verificar formato
            {

            }
            for (int i = 0; i < values.Count(); i++)//verificar formato
            {

            }
            if (!insertarArbol(tableName.Trim(), values, columns))
                return false;
            if (!insertarArchivoTabla(tableName.Trim(), values))
                return false;
            return true;
        }
        #endregion

        #region SELECT
        public List<string> listDataTable = new List<string>();
        public List<string> Missing = new List<string>();

        public bool Select(string[] columns, string tableName, int index)
        {
            try
            {
                listDataTable = new List<string>();
                string data = File.ReadAllText(path + "tablas\\" + tableName + ".tabla").Replace("\r\n", "$"); //cargar tabla
                // BTree<int, standardObject> tree = new BTree<int, standardObject>(tableName, 5); // cargar arbol
                string[] Table = data.Split('$');

                #region special case --> filtro llave primaria y mostar todo "*"
                bool fkey = false;
                if (Array.Exists(columns, element => element.StartsWith("WHERE")) &&
                    Array.Exists(columns, element => element.StartsWith("ID =")))
                {
                    fkey = true;
                }
                
                if (fkey) //Filtro a la llave primaria
                {
                    string[] keyRow = new string[2];
                    string key = "";

                    for (int k = 0; k < columns.Count(); k++)
                    {
                        if (columns[k].Trim() == "WHERE")
                        {
                            key = columns[k + 1].Replace("ID =", string.Empty);
                            break;
                        }
                    }

                    for (int k = 0; k < Table.Count(); k++)
                    {
                        string[] row = Table[k].Split(',');
                        if (row[0].Trim() == key.Trim())
                        {
                            keyRow[0] = Table[0];
                            keyRow[1] = Table[k];
                            break;
                        }
                    }
                    Table = keyRow;
                }


                if (columns[1].Trim() == "*") //Mostrar tabla completa
                {
                    if (Mostattod(columns, Table))
                        return true;
                    return false;
                }
                #endregion 

                List<string> showlst = new List<string>(); //Tabla para mostrar
                string[] strCol = Table[0].Split(','); //etiquetas columnas
                bool[] flags = new bool[9]; //banderas por columnas
                int[] orden = new int[9]; //Orden deseado de columnas
                string temp = "";

                #region preparations
                var Remove = new string[] { "(INT)", "(VARCHAR(100))", "(DATETIME)" };
                strCol = LimiarArray(strCol, Remove);
                for (int i = 1; i < strCol.Count(); i++)
                {
                    for (int j = 0; j < strCol.Count(); j++)
                    {
                        if (columns[i].Trim() == strCol[j].Trim())
                        {
                            flags[i - 1] = true;
                            temp = temp + columns[i] + ",";
                            orden[j] = i;
                            break;
                        }
                    }
                }

                temp = temp.TrimEnd(',');
                showlst.Add(temp);
                List<int> missing = new List<int>();
                Missing = new List<string>();

                if (!check(flags, columns).Item1)  //verificar la existencia de las columnas solicitadas
                {
                    missing = check(flags, columns).Item2;
                    for (int i = 0; i < missing.Count(); i++)
                    {
                        Missing.Add(columns[missing[i] + 1]);
                    }
                    return false;
                }
                int tablelenght = Table.Count() - 1;
                if(fkey)
                {
                    tablelenght = Table.Count();
                }
                #endregion
                
                for (int i = 1; i < tablelenght; i++)
                {
                    temp = "";
                    string[] row = Table[i].Split(',');

                    for (int j = 0; j < orden.Count(); j++)
                    {
                        int ix = orden[j] - 1;
                        if (ix >= 0)
                            temp = temp + row[ix] + ",";
                    }
                    temp = temp.TrimEnd(',');
                    showlst.Add(temp);
                }
                listDataTable = showlst;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public Tuple<bool, List<int>> check(bool[] flags, string[] lines)
        {
            int index = 0;
            List<int> missing = new List<int>();
            bool fg = true;
            for (int i = 1; i < lines.Count(); i++)
            {
                if (!fg)
                    break;
                if ("FROM" == lines[i].Trim())
                {
                    fg = false;
                }
                index++;

            }
            fg = true;
            for (int i = 0; i < index - 2; i++)
            {
                if (!flags[i])
                {
                    missing.Add(i);
                    fg = false;
                }
            }

            return Tuple.Create(fg, missing);
        }
        public bool Mostattod(string[] columns, string[] rows)
        {
            try
            {
                for (int i = 0; i < rows.Count(); i++)
                {
                    listDataTable.Add(rows[i]);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region DELETE
        public bool Delete(string[] Lines)
        {
            try
            {

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}

