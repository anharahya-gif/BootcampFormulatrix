using System.Text;

namespace AdvanceCSharp
{
    class Program
    {
        static void Main()
        {
            // Contoh penggunaan delegate
            Operation d = Add;
            d += Multiply;
            d -= Multiply;
            // menampilkan delegate
            Console.WriteLine(d(3, 3));
            // Contoh penggunaan delegate Notify
            Notify notify = SendEmail;
            notify("Hello from delegate!");
            // Contoh penggunaan Func dan Action
            Console.WriteLine("Func example: " + funcExample(5, 10));
            actionExample("Hello from Action delegate!");

            // Contoh penggunaan event
            var door = new Door();

            // WIRING
            door.DoorChanged += Door_DoorChanged;

            // Trigger
            door.Open();
            door.Close();
            door.Open();

            // Menghapus event handler
            door.DoorChanged -= Door_DoorChanged;
            door.Close();
            // tidak ada output karena event handler sudah dihapus
            door.DoorChanged += Door_DoorChanged;
            door.DoorChanged += (s, e) =>
            {
                if (e.IsOpen)
                    Console.WriteLine("🔔 Alarm armed");
                else
                    Console.WriteLine("🔕 Alarm disarmed");
            };
            door.DoorChanged += (s, e) =>
            {
                Console.WriteLine("📦 Logging door state");
            };
            door.Open();
            door.Close();
            door.DoorChanged -= Door_DoorChanged;

            var sensor = new TemperatureSensor();

            // subscribe
            sensor.TemperatureChanged += ACController_TemperatureChanged;
            sensor.TemperatureChanged += Logger_TemperatureChanged;
            sensor.TemperatureChanged += Sensor_TemperatureChanged;
            sensor.TemperatureChanged += (s, e) =>
            {
                // isi sendiri
                if (e.NewTemperature < 20)
                    Console.WriteLine("🔥 Heater ON");
                else
                    Console.WriteLine("🛑 Heater OFF");
            };
            // trigger
            sensor.SetTemperature(28);
            sensor.SetTemperature(32);
            sensor.SetTemperature(32); // tidak trigger
            sensor.SetTemperature(25);
            sensor.SetTemperature(10);


            // Coba Enumerable dan Iterator
            var numbers = new List<int> { 1, 2, 3 };

            var query = numbers.Where(x =>
            {
                Console.WriteLine($"Filter {x}");
                return x % 2 == 1;
            });

            Console.WriteLine("Before foreach");

            foreach (var n in query)
            {
                Console.WriteLine($"Result {n}");
            }

            Console.WriteLine("After foreach");

            var seq = GetData();

            Console.WriteLine(seq.First());
            Console.WriteLine(seq.First());
            Console.WriteLine("---------");

            var enumerable = Numbers();
            var enumerator = enumerable.GetEnumerator();

            Console.WriteLine("A");
            enumerator.MoveNext();
            Console.WriteLine(enumerator.Current);

            Console.WriteLine("B");
            enumerator.MoveNext();
            Console.WriteLine(enumerator.Current);

            Console.WriteLine("C");
            enumerator.MoveNext();
            Console.WriteLine("---------");
            var seqs = GetNumbers();

            var list = seqs.ToList();

            Console.WriteLine("First foreach");
            foreach (var x in list)
            {
                Console.WriteLine(x);
            }

            Console.WriteLine("Second foreach");
            foreach (var x in list)
            {
                Console.WriteLine(x);
            }

            // Coba Enumerable dan Iterator

            // Coba Nullable
            // ===== CHALLENGE 1: DISCOUNT =====
            Console.WriteLine("=== Discount ===");
            Console.WriteLine(CalculateFinalPrice(100, 20));   // 80
            Console.WriteLine(CalculateFinalPrice(100, 150));  // 0

            try
            {
                Console.WriteLine(CalculateFinalPrice(100, null));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // ===== CHALLENGE 2: AGE VALIDATION =====
            Console.WriteLine("\n=== Age Validation ===");
            Console.WriteLine(CanRegister(18, null)); // true
            Console.WriteLine(CanRegister(18, 21));   // false
            Console.WriteLine(CanRegister(25, 21));   // true

            // ===== CHALLENGE 3: SAFE AVERAGE =====
            Console.WriteLine("\n=== Safe Average ===");
            Console.WriteLine(SafeAverage(new List<int?> { 10, null, 20 })); // 15
            Console.WriteLine(SafeAverage(new List<int?> { null, null }));   // null

            // ===== CHALLENGE 4: BOOL? SECURITY =====
            Console.WriteLine("\n=== Access Check ===");
            Console.WriteLine(CanAccess(true));   // true
            Console.WriteLine(CanAccess(false));  // false
            Console.WriteLine(CanAccess(null));   // false

            // ===== BONUS =====
            Console.WriteLine("\n=== StringBuilder Length ===");
            StringBuilder sb = null;
            Console.WriteLine(GetLengthOrZero(sb)); // 0

            //Operator Overload
            //object a = 10;
            //object b = 20;

            //var c = a + b; // ❌ ERROR
            //Console.WriteLine(c);

            Money a = new Money(100);
            Money b = new Money(200);

            Money c = a + b; // ✅ OK
            Console.WriteLine(c.Amount);

            Point p1 = new Point(2, 3);
            Point p2 = new Point(4, 1);

            Point p3 = p1 + p2; // (6, 4)
            Console.WriteLine($"Point: ({p3.X}, {p3.Y})");




        }
        #region DelegateExample
        // Mendefinisikan delegate dengan nama operation
        delegate int Operation(int x, int y);
        // Metode yang sesuai dengan parameter delegate operation
        static int Add(int x, int y) => x + y;
        static int Multiply(int x, int y) => x * y;
        // Mendefinisikan delegate dengan nama Notify
        delegate void Notify(string message);
        // Metode yang sesuai dengan parameter delegate Notify
        public static void SendEmail(string message)
        {
            Console.WriteLine("Sending email: " + message);
        }
        #endregion
        //Coba func dan action
        #region FuncAndActionExample    
        static Func<int, int, int> funcExample = (x, y) => x + y;
        static Action<string> actionExample = message => Console.WriteLine("Action says: " + message);
        #endregion

        // Coba event

        #region EventExample
        //Publisher
        public class Door
        {
            private bool isOpen;

            public event EventHandler<DoorChangedEventArgs> DoorChanged;

            public void Open()
            {
                if (isOpen) return;
                isOpen = true;
                OnDoorChanged(isOpen);
            }

            public void Close()
            {
                if (!isOpen) return;
                isOpen = false;
                OnDoorChanged(isOpen);
            }

            protected virtual void OnDoorChanged(bool isOpen)
            {
                DoorChanged?.Invoke(this, new DoorChangedEventArgs(isOpen));
            }
        }
        public class DoorChangedEventArgs : EventArgs
        {
            public bool IsOpen { get; }

            public DoorChangedEventArgs(bool isOpen)
            {
                IsOpen = isOpen;
            }
        }
        //subscriber
        static void Door_DoorChanged(object sender, DoorChangedEventArgs e)
        {
            Console.WriteLine(
                e.IsOpen ? "Door is OPEN" : "Door is CLOSED"
            );
        }

        public class TemperatureChangedEventArgs : EventArgs
        {
            public double OldTemperature { get; }
            public double NewTemperature { get; }

            public TemperatureChangedEventArgs(double oldTemp, double newTemp)
            {
                OldTemperature = oldTemp;
                NewTemperature = newTemp;
            }
        }
        public class TemperatureSensor
        {
            private double temperature;

            public event EventHandler<TemperatureChangedEventArgs> TemperatureChanged;

            public void SetTemperature(double newTemp)
            {
                if (temperature == newTemp) return;

                double oldTemp = temperature;
                temperature = newTemp;

                OnTemperatureChanged(oldTemp, newTemp);
            }

            protected virtual void OnTemperatureChanged(double oldTemp, double newTemp)
            {
                TemperatureChanged?.Invoke(
                    this,
                    new TemperatureChangedEventArgs(oldTemp, newTemp)
                );
            }
        }
        static void ACController_TemperatureChanged(
        object sender,
        TemperatureChangedEventArgs e)
        {
            if (e.NewTemperature > 30)
                Console.WriteLine("❄️ AC ON");
            else
                Console.WriteLine("🛑 AC OFF");
        }
        static void Logger_TemperatureChanged(
            object sender,
            TemperatureChangedEventArgs e)
        {
            Console.WriteLine(
                $"🌡️ Temp changed: {e.OldTemperature} → {e.NewTemperature}"
            );
        }
        static void Sensor_TemperatureChanged(
            object sender,
            TemperatureChangedEventArgs e)
        {
            if (Math.Abs(e.NewTemperature - e.OldTemperature) > 5)

            {
                Console.WriteLine("⚠️ Temperature spike detected !");
            }
            Console.WriteLine(
                $"[SENSOR LOG] Temp changed from {e.OldTemperature} to {e.NewTemperature}"
            );
        }

        #endregion

        #region EnumerableAndIteratorExample
        static IEnumerable<int> GetData()
        {
            Console.WriteLine("Generating");
            yield return 42;
        }
        static IEnumerable<int> Numbers()
        {
            Console.WriteLine("Run");
            yield return 1;
            yield return 2;
        }
        static IEnumerable<int> GetNumbers()
        {
            Console.WriteLine("Generating");
            yield return 1;
            yield return 2;
        }
        #endregion
        #region NullableChallengeSolutions
        // CHALLENGE 1
        static int CalculateFinalPrice(int price, int? discount)
        {
            if (!discount.HasValue)
                throw new InvalidOperationException("Discount belum tersedia");

            if (discount < 0)
                throw new ArgumentException("Discount tidak boleh negatif");

            if (discount >= price)
                return 0;

            return price - discount.Value;
        }

        // CHALLENGE 2
        static bool CanRegister(int age, int? minAge)
        {
            return minAge == null || age >= minAge;
        }

        // CHALLENGE 3
        static double? SafeAverage(List<int?> values)
        {
            var validValues = values
                .Where(v => v.HasValue)
                .Select(v => v.Value)
                .ToList();

            if (!validValues.Any())
                return null;

            return validValues.Average();
        }

        // CHALLENGE 4
        static bool CanAccess(bool? isAdmin)
        {
            return isAdmin == true;
        }

        // BONUS
        static int GetLengthOrZero(StringBuilder sb)
        {
            return sb?.Length ?? 0;
        }
        #endregion
        #region OperatorOverloadExample
        public struct Money
        {
            public decimal Amount;

            public Money(decimal amount)
            {
                Amount = amount;
            }

            public static Money operator +(Money a, Money b)
            {
                return new Money(a.Amount + b.Amount);
            }
        }
        public struct Point
        {
            public int X;
            public int Y;

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static Point operator +(Point a, Point b)
            {
                return new Point(a.X + b.X, a.Y + b.Y);
            }
        }

        #endregion          
    }
}
