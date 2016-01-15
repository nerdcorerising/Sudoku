using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public class Position
    {
        private bool _solved = false;
        private int _knownPos = 0;
        private bool[] _possibles = new bool[9];

        public Position(int num)
        {
            if (num == 0)
            {
                for (int i = 0; i < _possibles.Length; ++i)
                {
                    _possibles[i] = true;
                }
            }
            else
            {
                for (int i = 0; i < _possibles.Length; ++i)
                {
                    _possibles[i] = false;
                }

                _possibles[num - 1] = true;
                _solved = true;
                _knownPos = num;
            }
        }

        public bool IsSolved
        {
            get
            {
                return _solved;
            }
        }

        public int KnownPosition
        {
            get
            {
                return _knownPos;
            }
        }

        public bool IsPossible(int num)
        {
            Debug.Assert(num >= 0 && num <= _possibles.Length);
            return _possibles[num - 1];
        }

        public void RemovePossible(int num)
        {
            Debug.Assert(num >= 0 && num <= _possibles.Length);
            if (!_possibles[num - 1])
            {
                return;
            }

            _possibles[num - 1] = false;

            bool single = false;
            bool multiple = false;
            int pos = 0;
            for (int i = 0; i < _possibles.Length; ++i)
            {
                if (_possibles[i])
                {
                    if (single)
                    {
                        multiple = true;
                        break;
                    }
                    else
                    {
                        single = true;
                    }

                    pos = i + 1;
                }
            }

            if (multiple)
            {
                return;
            }

            Debug.Assert(single);
            _knownPos = pos;
            _solved = true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Provide input file.");
                return;
            }
            else if (args.Length != 1)
            {
                Console.WriteLine("Assuming first argument is an input file, ignoring all other arguments.");
            }

            Position[] sudoku = null;
            try
            {
                sudoku = ReadFile(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Error reading file: {0}", e.Message));
                return;
            }

            while (!IsSolved(sudoku))
            {
                for (int i = 0; i < sudoku.Length; ++i)
                {
                    if (sudoku[i].IsSolved)
                    {
                        SetRowForPos(sudoku, i, sudoku[i].KnownPosition);
                        SetColForPos(sudoku, i, sudoku[i].KnownPosition);
                        SetBlockForPos(sudoku, i, sudoku[i].KnownPosition);
                    }
                }
            }

            for (int i = 0; i < sudoku.Length; ++i)
            {
                Console.Write("{0} ", sudoku[i].KnownPosition);

                if (i % 9 == 8)
                {
                    Console.WriteLine();
                }
            }
        }

        private static void SetRowForPos(Position[] sudoku, int pos, int num)
        {
            int row = pos / 9;

            for (int i = 0; i < 9; ++i)
            {
                int candidate = (row * 9) + i;
                if (candidate != pos)
                {
                    sudoku[candidate].RemovePossible(num);
                }
            }
        }

        private static void SetColForPos(Position[] sudoku, int pos, int num)
        {
            int col = pos % 9;

            for (int i = 0; i < 9; ++i)
            {
                int candidate = (i * 9) + col;
                if (candidate != pos)
                {
                    sudoku[candidate].RemovePossible(num);
                }
            }
        }

        private static int RoundToThree(int num)
        {
            switch (num)
            {
                case 0:
                case 1:
                case 2:
                    return 0;
                case 3:
                case 4:
                case 5:
                    return 3;
                case 6:
                case 7:
                case 8:
                    return 6;
                default:
                    throw new Exception("Unexpected number");
            }
        }

        private static void SetBlockForPos(Position[] sudoku, int pos, int num)
        {
            int startRow = RoundToThree(pos / 9);
            int startCol = RoundToThree(pos % 9);

            for (int row = startRow; row < startRow + 3; ++row)
            {
                for (int col = startCol; col < startCol + 3; ++col)
                {
                    int candidate = (row * 9) + col;
                    if (candidate != pos)
                    {
                        sudoku[candidate].RemovePossible(num);
                    }
                }
            }
        }

        private static bool IsSolved(Position[] sudoku)
        {
            foreach (Position p in sudoku)
            {
                if (!p.IsSolved)
                {
                    return false;
                }
            }

            return true;
        }

        private static Position[] ReadFile(string fileName)
        {
            Position[] sudoku = new Position[9 * 9];
            using (StreamReader input = new StreamReader(new FileStream(fileName, FileMode.Open)))
            {
                string line;
                int lineCount = 0;
                while ((line = input.ReadLine()) != null)
                {
                    if (lineCount >= 9)
                    {
                        throw new Exception("More than 9 lines found in puzzle.");
                    }

                    if (line.Length != 9)
                    {
                        throw new Exception(String.Format("Line {0} is length {1}, expected 9.", lineCount, line.Length));
                    }

                    for (int i = 0; i < line.Length; ++i)
                    {
                        int num;
                        if (!int.TryParse(line[i].ToString(), out num))
                        {
                            throw new Exception(String.Format("Found non-integer in puzzle at line {0} pos {1}", lineCount, i));
                        }

                        sudoku[(lineCount * 9) + i] = new Position(num);
                    }

                    ++lineCount;
                }

                return sudoku;
            }
        }
    }
}
