using System;
using System.Threading.Tasks;
using Akka.Streams;
using Akka.Streams.Dsl;

namespace TestApp.AkkaRedux;

public static class AkkaTestApp
{
    public static async Task AkkaMain()
    {
        ActorMaterializer materializer = null!;

        var pairUpWithToString = Flow.FromGraph(
            GraphDsl.Create(b =>
                            {
                                // prepare graph elements
                                var broadcast = b.Add(new Broadcast<int>(2));
                                var zip = b.Add(new Zip<int, string>());

                                // connect the graph
                                b.From(broadcast.Out(0)).Via(Flow.Create<int>().Select(x => x)).To(zip.In0);
                                b.From(broadcast.Out(1)).Via(Flow.Create<int>().Select(x => x.ToString())).To(zip.In1);

                                // expose ports
                                return new FlowShape<int, (int, string)>(broadcast.In, zip.Out);
                            }));
        
        
        var pickMaxOfThree = GraphDsl.Create(b =>
                                             {
                                                 var zip1 = b.Add(ZipWith.Apply<int, int, int>(Math.Max));
                                                 var zip2 = b.Add(ZipWith.Apply<int, int, int>(Math.Max));
                                                 b.From(zip1.Out).To(zip2.In0);

                                                 return new UniformFanInShape<int, int>(zip2.Out, zip1.In0, zip1.In1, zip2.In1);
                                             });

        var resultSink = Sink.First<int>();

        var g = RunnableGraph.FromGraph(GraphDsl.Create(resultSink, (b, sink) =>
                                                                    {
                                                                        // importing the partial graph will return its shape (inlets & outlets)
                                                                        var pm3 = b.Add(pickMaxOfThree);
                                                                        var s1 = Source.Single(1).MapMaterializedValue<Task<int>>(_ => null);
                                                                        var s2 = Source.Single(2).MapMaterializedValue<Task<int>>(_ => null);
                                                                        var s3 = Source.Single(3).MapMaterializedValue<Task<int>>(_ => null);

                                                                        b.From(s1).To(pm3.In(0));
                                                                        b.From(s2).To(pm3.In(1));
                                                                        b.From(s3).To(pm3.In(2));

                                                                        b.From(pm3.Out).To(sink.Inlet);

                                                                        return ClosedShape.Instance;
                                                                    }));

        
        
        var max = g.Run(materializer);
    }
}